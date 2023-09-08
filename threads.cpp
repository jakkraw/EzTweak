#define _CRT_SECURE_NO_WARNINGS

#include <windows.h>
#include <vector>
#include <stdio.h>
#include <iostream>
#include <string>
#include <sstream>
#include <algorithm>
#include <processthreadsapi.h>
#include <Tlhelp32.h>
#include <tchar.h>
#include <unordered_map>

#define STATUS_SUCCESS ((NTSTATUS)0x00000000L)
#define ThreadQuerySetWin32StartAddress 9

typedef NTSTATUS(WINAPI* NTQUERYINFOMATIONTHREAD)(HANDLE, LONG, PVOID, ULONG, PULONG);
typedef HRESULT(WINAPI* GETTHREADDESCRIPTION)(HANDLE, PWSTR*);

DWORD GetProcessPIDByName(PCWSTR name)
{
	DWORD pid = 0;

	// Create toolhelp snapshot.
	HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	PROCESSENTRY32 process;
	ZeroMemory(&process, sizeof(process));
	process.dwSize = sizeof(process);

	// Walkthrough all processes.
	if (Process32First(snapshot, &process))
	{
		do
		{
			// Compare process.szExeFile based on format of name, i.e., trim file path
			// trim .exe if necessary, etc.
			if (std::wstring{process.szExeFile} == std::wstring(name))
			{
				pid = process.th32ProcessID;
				break;
			}
		} while (Process32Next(snapshot, &process));
	}

	CloseHandle(snapshot);

	return pid;
}

void terminate(std::wstring name){
	DWORD pid{ GetProcessPIDByName(name.c_str()) };
	if (!pid) {
		return;
	}
	auto handle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
	TerminateProcess(handle, 0);
	CloseHandle(handle);
}


BOOL MatchAddressToModule(DWORD dwProcId, std::wstring& lpstrModule, DWORD64 dwThreadStartAddr, PDWORD64 pModuleStartAddr, PMODULEENTRY32W module) // by Echo
{
	BOOL bRet = FALSE;
	HANDLE hSnapshot;
	MODULEENTRY32 moduleEntry32;

	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPALL, dwProcId);

	moduleEntry32.dwSize = sizeof(MODULEENTRY32);
	moduleEntry32.th32ModuleID = 1;

	if (Module32First(hSnapshot, &moduleEntry32)) {
		if (dwThreadStartAddr >= (DWORD64)moduleEntry32.modBaseAddr && dwThreadStartAddr <= ((DWORD64)moduleEntry32.modBaseAddr + moduleEntry32.modBaseSize)) {
			lpstrModule = std::wstring{moduleEntry32.szExePath};
			*module = moduleEntry32;
		}
		else {
			while (Module32Next(hSnapshot, &moduleEntry32)) {
				if (dwThreadStartAddr >= (DWORD64)moduleEntry32.modBaseAddr && dwThreadStartAddr <= ((DWORD64)moduleEntry32.modBaseAddr + moduleEntry32.modBaseSize)) {
					lpstrModule = std::wstring{moduleEntry32.szExePath};
					*module = moduleEntry32;
					break;
				}
			}
		}
	}

	if (pModuleStartAddr) *pModuleStartAddr = (DWORD64)moduleEntry32.modBaseAddr;
	CloseHandle(hSnapshot);

	return bRet;
}

DWORD64 WINAPI GetThreadStartAddress(HANDLE hThread) // by Echo
{
	NTSTATUS ntStatus;
	DWORD64 dwThreadStartAddr = 0;
	HANDLE hPeusdoCurrentProcess, hNewThreadHandle;
	NTQUERYINFOMATIONTHREAD NtQueryInformationThread;

	if ((NtQueryInformationThread = (NTQUERYINFOMATIONTHREAD)GetProcAddress(GetModuleHandle(L"ntdll.dll"), "NtQueryInformationThread"))) {
		hPeusdoCurrentProcess = GetCurrentProcess();
		if (DuplicateHandle(hPeusdoCurrentProcess, hThread, hPeusdoCurrentProcess, &hNewThreadHandle, THREAD_QUERY_INFORMATION, FALSE, 0)) {
			ntStatus = NtQueryInformationThread(hNewThreadHandle, ThreadQuerySetWin32StartAddress, &dwThreadStartAddr, sizeof(DWORD64), NULL);
			CloseHandle(hNewThreadHandle);
			if (ntStatus != STATUS_SUCCESS) return 0;
		}

	}

	return dwThreadStartAddr;
}


long long toLong(const FILETIME& t)
{
	return LARGE_INTEGER{ t.dwLowDateTime, (long)t.dwHighDateTime }.QuadPart;
}

struct ThreadInfo {
	DWORD64 pid{ 0 };
	DWORD64 id{ 0 };
	HANDLE handle{ INVALID_HANDLE_VALUE };
	std::wstring desc{};
	long long creation_time;
	long long exit_time;
	long long kernel_time;
	long long user_time;
	int iopending{ -1 };
	DWORD64 dcpol{ 0 };
	DWORD64 abcpuprio{ 0 };
	DWORD64 memprio{ 0 };
	DWORD64 staddr{ 13 };
	DWORD64 offset{ 0 };
	std::wstring moduleName;
	MODULEENTRY32W m;
	THREADENTRY32 te;
	int priority{ 0 };
	CONTEXT context;

	ThreadInfo(DWORD64 id, DWORD64 pid, THREADENTRY32 te) : id{ id }, pid{ pid }, te{ te } {
		update();
	}

	void update() {
		handle = ::OpenThread(THREAD_ALL_ACCESS, FALSE, id);
		staddr = GetThreadStartAddress(handle);

		DWORD64 dwModuleBaseAddr{ 0 };
		MatchAddressToModule(pid, moduleName, staddr, &dwModuleBaseAddr, &m);

		offset = staddr - dwModuleBaseAddr;
		
		auto GetThreadDescription = (GETTHREADDESCRIPTION)GetProcAddress(GetModuleHandle(L"KernelBase.dll"), "GetThreadDescription");
		
		WCHAR* r = nullptr;
		HRESULT hr = GetThreadDescription(handle, &r);
		if (r != nullptr) { desc = std::wstring{ r }; }

		LocalFree(r);
		FILETIME creation;
		FILETIME exit;
		FILETIME kernel;
		FILETIME user;
		GetThreadTimes(handle, &creation, &exit, &kernel, &user);
		creation_time = toLong(creation);
		exit_time = toLong(exit);
		kernel_time = toLong(kernel);
		user_time = toLong(user);
		GetThreadIOPendingFlag(handle, &iopending);
		bool return_value = GetThreadInformation(
			handle,
			ThreadDynamicCodePolicy,
			&dcpol,
			sizeof(DWORD)
		);

		return_value = GetThreadInformation(
			handle,
			ThreadAbsoluteCpuPriority,
			&abcpuprio,
			sizeof(DWORD)
		);

		return_value = GetThreadInformation(
			handle,
			ThreadMemoryPriority,
			&memprio,
			sizeof(DWORD)
		);

		priority = GetThreadPriority(handle);

		auto pc = GetPriorityClass(handle);
		BOOL p{ false };
		return_value = GetThreadPriorityBoost(handle, &p);

		GetThreadContext(handle, &context);
		CloseHandle(handle);
	}

	void print() const {
		std::wcout << "------------------------" << std::endl;
		std::wcout << "ID: " << id << std::endl;
		std::wcout << "Description: " << desc << std::endl;
		std::wcout << "priority:" << priority << " iopending:" << iopending << " dcodepol:" << dcpol << " abcpuprio:" << abcpuprio << " memprio:" << memprio << std::endl;
		std::wcout << "tpBasePri:" << te.tpBasePri << " tpDeltaPri:" << te.tpDeltaPri << " cntUsage:" << te.cntUsage << " dwFlags:" << te.dwFlags << " size:" << te.dwSize << std::endl;
		std::wcout << "kt:" << kernel_time << " ut:" << user_time << " et:" << exit_time << " ct:" << creation_time << std::endl;
		std::wcout << "StartAddress: " << std::hex << staddr << std::dec << std::endl;
		std::wcout << "Module: " << moduleName << "+0x" << std::hex << offset << std::dec << std::endl;
		std::wcout << "glibcntusage:" << m.GlblcntUsage << " proccntusage:" << m.ProccntUsage << " mid:" << m.th32ModuleID << " szmodule:" << m.szModule << std::endl;
		std::wcout << "context:" << context.ContextFlags << std::endl;
		std::wcout << "------------------------" << std::endl;
	}

	void printShort() const {
		std::wcout << id << "\t" << moduleName << "+0x" << std::hex << offset << std::dec << std::endl;
	}

	bool suspend() {
		handle = ::OpenThread(THREAD_ALL_ACCESS, FALSE, id);
		DWORD dwSuspendCount = SuspendThread(handle);
		ResumeThread(handle);
		if (dwSuspendCount == 0) {
			dwSuspendCount = SuspendThread(handle);
		}
		CloseHandle(handle);
		std::wcout << "suspending id:" << id << ", code:" << (long int)dwSuspendCount << std::endl;
		return dwSuspendCount > -1;
	}

	bool terminate() {
		handle = ::OpenThread(THREAD_ALL_ACCESS, FALSE, id);
		auto dwSuspendCount = TerminateThread(handle, 0);
		CloseHandle(handle);
		std::wcout << "terminating id:" << id << ", code:" << (long int)dwSuspendCount << std::endl;
		return dwSuspendCount > -1;
	}

	bool set_priority() {
		handle = ::OpenThread(THREAD_ALL_ACCESS, FALSE, id);
		SetThreadPriority(handle, THREAD_PRIORITY_TIME_CRITICAL);
		CloseHandle(handle);
		return true;
	}

	int get_priority() {
		handle = ::OpenThread(THREAD_ALL_ACCESS, FALSE, id);
		auto p = GetThreadPriority(handle);
		CloseHandle(handle);
		return p;
	}

};


std::vector<ThreadInfo> listProcessThreads(DWORD targetProcessId)
{
	std::vector<ThreadInfo> threads;
	HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	if (h == INVALID_HANDLE_VALUE)
	{
		return threads;
	}
	THREADENTRY32 te;
	te.dwSize = sizeof(te);
	if (Thread32First(h, &te))
	{
		do
		{
			if (te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(te.th32OwnerProcessID))
			{

				if (te.th32OwnerProcessID == targetProcessId) {
					threads.emplace_back(te.th32ThreadID, te.th32OwnerProcessID, te);
				}
			}
			te.dwSize = sizeof(te);
		} while (Thread32Next(h, &te));
	}
	CloseHandle(h);
	return threads;

}

bool filterRegSpam(const ThreadInfo& ti) {
	if (std::wstring{ ti.m.szModule } == L"RocketLeague.exe") {
		if (ti.kernel_time != 0 && ti.user_time != 0 && ti.exit_time == 0 && ti.creation_time != 0) {
			if (ti.staddr == 0x7ff7d4ed2730) {
				if (ti.offset == 0x452730) {
					if (ti.user_time > ti.kernel_time) {
						return true;
					}
				}
			}
		}
	}
	
	
	
	return false;
}

bool filterOverlay(const ThreadInfo& ti) {
	if (std::wstring{ ti.m.szModule } == L"EOSSDK-Win64-Shipping.dll") {
		if (ti.exit_time == 0 && ti.creation_time != 0) {
			//if (!ti.desc.empty())
				return true;
		}
	}
	
	if (std::wstring{ ti.m.szModule } == L"dxgi.dll") {
		if (ti.exit_time == 0 && ti.creation_time != 0) {
			if (ti.staddr == 0x7ffc1c2b5b20) {
				if (ti.offset == 0x65b20) {
						return true;
				}
			}
		}
	}
	
	return false;
}

int main(int argc, wchar_t** argv) {
	const std::wstring binary = argc > 1 ? argv[1] :  L"RocketLeague.exe";

	while (true) {
		auto pid = GetProcessPIDByName(binary.c_str());
		auto hh = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
		SetPriorityClass(hh, REALTIME_PRIORITY_CLASS);
		for (auto& t : listProcessThreads(pid)) {
			if (t.get_priority() != REALTIME_PRIORITY_CLASS) {
				t.set_priority();
				t.printShort();
			}
		}
		CloseHandle(hh);
		Sleep(1000);
	}

	return 0;
}

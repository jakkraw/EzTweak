namespace EzTweak
{
    public class ServiceTweak : RegistryTweak
    {
        string service;

        public ServiceTweak(string name, string description, string service, string on_value, string off_value) : base(name, description, ActionType.SERVICE, registry_path(service), on_value, off_value)
        {
            this.service = service;
        }

        private static string registry_path(string service)
        {
            return $@"HKLM\SYSTEM\CurrentControlSet\Services\{service}\Start";
        }

        private string alias(string value)
        {
            if (sanitize("4") == value)
            {
                return "(Disabled)";
            }

            if (sanitize("3") == value)
            {
                return "(Manual)";
            }

            if (sanitize("2") == value)
            {
                return "(Automatic)";
            }
            return "";
        }

        public override string status()
        {
            var value = current_value();
            return $"{service} is {alias(value)}";
        }
    }
}

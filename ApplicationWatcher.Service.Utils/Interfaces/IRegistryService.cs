namespace ApplicationWatcher.Service.Utils.Interfaces
{
    public interface IRegistryService
    {
        T GetRegistryValue<T>(string basePath, string valueName);
    }
}

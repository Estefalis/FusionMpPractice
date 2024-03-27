public interface IPersistentData
{
    bool SaveData<T>(string _subFolder, string _fileName, string _fileFormat, T data, bool _encrypted, bool _overwriteFile = true);
    T LoadData<T>(string _subFolder, string _fileName, string _fileFormat, bool _encrypted);
}
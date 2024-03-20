using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class SerializingData : IPersistentData
{
    private const string m_KEY = "Yx/P5QVTRuUt55p82QNnkI1LXbXM4/qsxM9P7eihc0o=";
    private const string m_IV = "L5j2EvGAywqpH86whhvjWA=="; //InitializationVector.

    public bool SaveData<T>(string _subFolder, string _fileName, string _fileFormat, T _data, bool _encrypted, bool _overwriteFile = true)
    {
        #region Directory Check
        if (!Directory.Exists(Application.persistentDataPath + _subFolder))
        {
#if UNITY_EDITOR
            Debug.Log("SubFolder(s) do(es) not exist. Creating... .");
#endif
            Directory.CreateDirectory(Application.persistentDataPath + _subFolder);
        }
        #endregion

        string combinedPath = Application.persistentDataPath + _subFolder + _fileName + _fileFormat;

        #region Overwrite Check
        if (File.Exists(combinedPath) && !_overwriteFile)
        {
#if UNITY_EDITOR
            Debug.Log("The data will not be saved, due to settings of the user. Exiting.");
#endif
            return false;
        }
        #endregion

        try
        {
            if (File.Exists(combinedPath))
            {
#if UNITY_EDITOR
                Debug.Log("The data already exists. Deleting the old file and writing a new one.");
#endif
                File.Delete(combinedPath);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Creating a new file. Gimme the amount of milliseconds needed for it. Thank you!");
#endif
            }

            using FileStream stream = File.Create(combinedPath);

            if (_encrypted)
            {
                WriteEncryptedData(_data, stream);
            }
            else
            {
                stream.Close();
                //TODO: Place to add more Serialize options. (switch with _fileFormat?)
                File.WriteAllText(combinedPath, JsonConvert.SerializeObject(_data, Formatting.Indented));
            }

            return true;
        }
        catch (Exception _exception)
        {
            Debug.LogError($"Cannot save the data, because of {_exception.Message} {_exception.StackTrace}.");
            return false;
        }
    }

    public T LoadData<T>(string _subFolder, string _fileName, string _fileFormat, bool _encrypted)
    {
        #region Directory Check
        if (!Directory.Exists(Application.persistentDataPath + _subFolder))
        {
#if UNITY_EDITOR
            Debug.Log("SubFolder(s) do(es) not exist. Creating... .");
#endif
            Directory.CreateDirectory(Application.persistentDataPath + _subFolder);
        }
        #endregion

        string path = Application.persistentDataPath + _subFolder + _fileName + _fileFormat;

        if (!File.Exists(path))
        {
            //Debug.LogError($"The file at {path} cannot be loaded, because it does not exist!");
            //throw new FileNotFoundException($"{path} does not exit!");

            //The receiver can check for/and react to 'Null'.
            return default;
        }

        try
        {
            T data;

            if (_encrypted)
            {
                data = ReadEncryptedData<T>(path);
            }
            else
            {
                //TODO: Place to add more Deserialize options. (switch with _fileFormat?)
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }

            return data;
        }
        catch (Exception _exception)
        {
            Debug.LogError($"Failed to load the data, due to {_exception.Message} {_exception.StackTrace}.");
            throw _exception;
        }
    }

    private void WriteEncryptedData<T>(T _data, FileStream _stream)
    {
        using Aes aesEncryptionProvider = Aes.Create();
        //Has to be commented out, to use the 2 Debug.Logs below, to get new Key and IV.___
        aesEncryptionProvider.Key = Convert.FromBase64String(m_KEY);
        aesEncryptionProvider.IV = Convert.FromBase64String(m_IV);
        //_________________________________________________________________________________

        using ICryptoTransform cryptoTransform = aesEncryptionProvider.CreateEncryptor();
        using CryptoStream cryptoStream = new CryptoStream(_stream, cryptoTransform, CryptoStreamMode.Write);

        //OneTime use for Key and IV creation set, on top of this class:_____________________________
        //Debug.Log($" Key: {Convert.ToBase64String(aesEncryptionProvider.Key)}");
        //Debug.Log($"InitializeBasic Vector: {Convert.ToBase64String(aesEncryptionProvider.IV)}");
        //___________________________________________________________________________________________

        //Encoding.ASCII in the Tutorial 'https://www.youtube.com/watch?v=mntS45g8OK4'.
        //TODO: Place to add more Serialize options. (switch with _fileFormat?)
        cryptoStream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_data, Formatting.Indented)));
    }

    private T ReadEncryptedData<T>(string _path)
    {
        byte[] fileBytes = File.ReadAllBytes(_path);

        using Aes aesDecryptionProvider = Aes.Create();
        aesDecryptionProvider.Key = Convert.FromBase64String(m_KEY);
        aesDecryptionProvider.IV = Convert.FromBase64String(m_IV);

        using ICryptoTransform cryptoTransform = aesDecryptionProvider.CreateDecryptor(aesDecryptionProvider.Key, aesDecryptionProvider.IV);
        using MemoryStream decryptionStream = new(fileBytes);
        using CryptoStream cryptoStream = new(decryptionStream, cryptoTransform, CryptoStreamMode.Read);

        using StreamReader reader = new(cryptoStream);
        string decryptedData = reader.ReadToEnd();

        Debug.Log($"Decrypted data. On any error check used KEY and/or Initialization Vector: {decryptedData}.");
        //TODO: Place to add more Deserialize options. (switch with _fileFormat?)
        return JsonConvert.DeserializeObject<T>(decryptedData);
    }
}
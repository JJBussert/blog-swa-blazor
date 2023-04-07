//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.IO;

//public class EnvFileConfigurationProvider : ConfigurationProvider
//{
//    private readonly string _envFilePath;

//    public EnvFileConfigurationProvider(string envFilePath)
//    {
//        _envFilePath = envFilePath;
//    }

//    public override void Load()
//    {
//        if (!File.Exists(_envFilePath))
//        {
//            return;
//        }

//        var lines = File.ReadAllLines(_envFilePath);

//        foreach (var line in lines)
//        {
//            var keyValue = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);

//            if (keyValue.Length == 2)
//            {
//                string key = keyValue[0].Trim();
//                string value = keyValue[1].Trim();

//                if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 1)
//                {
//                    value = value.Substring(1, value.Length - 2);
//                }

//                Data[key] = value;
//            }
//        }
//    }
//}

//public class EnvFileConfigurationSource : IConfigurationSource
//{
//    private readonly string _envFilePath;

//    public EnvFileConfigurationSource(string envFilePath)
//    {
//        _envFilePath = envFilePath;
//    }

//    public IConfigurationProvider Build(IConfigurationBuilder builder)
//    {
//        return new EnvFileConfigurationProvider(_envFilePath);
//    }
//}

//public static class EnvFileConfigurationExtensions
//{
//    public static IConfigurationBuilder AddEnvFile(this IConfigurationBuilder builder, string envFilePath)
//    {
//        return builder.Add(new EnvFileConfigurationSource(envFilePath));
//    }
//}

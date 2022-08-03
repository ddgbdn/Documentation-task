using System;
using System.Linq;
using System.Reflection;

namespace Documentation
{
    public class Specifier<T> : ISpecifier
    {
        public string GetApiDescription()
        {
            return typeof(T).GetCustomAttribute<ApiDescriptionAttribute>()?.Description;
        }

        public string[] GetApiMethodNames()
        {
            return typeof(T)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetCustomAttribute<ApiMethodAttribute>() != null)
                .Select(m => m.Name)
                .ToArray();
        }

        public string GetApiMethodDescription(string methodName)
        {
            return typeof(T)
                .GetMethod(methodName)?
                .GetCustomAttribute<ApiDescriptionAttribute>()?
                .Description;
        }

        public string[] GetApiMethodParamNames(string methodName)
        {
            return typeof(T)
                .GetMethod(methodName)?
                .GetParameters()
                .Select(p => p.Name)
                .ToArray();
        }

        public string GetApiMethodParamDescription(string methodName, string paramName)
        {
            return typeof(T)
                .GetMethod(methodName)?
                .GetParameters()
                .Where(p => p.Name == paramName)
                .SingleOrDefault()?
                .GetCustomAttribute<ApiDescriptionAttribute>()?
                .Description;
        }

        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
        {            
            var param = typeof(T).GetMethod(methodName)?
                .GetParameters()
                .Where(p => p.Name == paramName)
                .SingleOrDefault();
            var result = new ApiParamDescription
            {
                ParamDescription = new CommonDescription(paramName,
                    GetApiMethodParamDescription(methodName, paramName)),
                Required = param?.GetCustomAttribute<ApiRequiredAttribute>()?.Required ?? false,
                MinValue = param?.GetCustomAttribute<ApiIntValidationAttribute>()?.MinValue ?? null,
                MaxValue = param?.GetCustomAttribute<ApiIntValidationAttribute>()?.MaxValue ?? null
            };
            return result;
        }

        public ApiMethodDescription GetApiMethodFullDescription(string methodName)
        {
            var method = typeof(T).GetMethods()
                .Where(m => m.Name == methodName && m.GetCustomAttributes<ApiMethodAttribute>().Any())
                .SingleOrDefault();
            if (method == null) return null;
            var returnParam = method.ReturnParameter;
            return new ApiMethodDescription
            {
                MethodDescription = new CommonDescription(methodName, GetApiMethodDescription(methodName)),
                ParamDescriptions = method.GetParameters()
                    .Select(p => GetApiMethodParamFullDescription(methodName, p.Name))
                    .ToArray(),
                ReturnDescription = returnParam.CustomAttributes.Any()
                    ? new ApiParamDescription
                    {
                        ParamDescription = new CommonDescription(),
                        MaxValue = returnParam.GetCustomAttribute<ApiIntValidationAttribute>()?.MaxValue ?? null,
                        MinValue = returnParam.GetCustomAttribute<ApiIntValidationAttribute>()?.MinValue ?? null,
                        Required = returnParam.GetCustomAttribute<ApiRequiredAttribute>()?.Required ?? false
                    }
                    : null
            };
        }
    }
}
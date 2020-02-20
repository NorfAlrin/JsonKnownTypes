using System;
using System.Collections.Generic;
using System.Linq;
using JsonKnownTypes.Exceptions;

namespace JsonKnownTypes
{
    public static class JsonKnownSettingsService
    {
        public static JsonKnownDiscriminatorAttribute DefaultDiscriminatorAttribute { get; set; } = new JsonKnownDiscriminatorAttribute();
        public static Func<string, string> DefaultNaming { get; set; } = name => name;

        public static JsonKnownTypeSettings GetSettings<T>()
        {
            var data = new JsonKnownTypeSettings();
            var knownDiscriminatorAttribute = (JsonKnownDiscriminatorAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(JsonKnownDiscriminatorAttribute)) 
                                              ?? DefaultDiscriminatorAttribute;
            
            data.Name = knownDiscriminatorAttribute.Name;
            var autoType = knownDiscriminatorAttribute.AutoJsonKnownType;

            var knownTypeAttributes = (JsonKnownTypeAttribute[])Attribute.GetCustomAttributes(typeof(T), typeof(JsonKnownTypeAttribute));
            try
            {
                data.TypeToDiscriminator = knownTypeAttributes.ToDictionary(x => x.Type, x => x.Discriminator);

                if(autoType)
                    AddTypesWhichAreNotContains<T>(data.TypeToDiscriminator);

                data.DiscriminatorToType = data.TypeToDiscriminator.ToDictionary(x => x.Value, x => x.Key);
            }
            catch (ArgumentException e)
            {
                throw new AttributeArgumentException(e.Message, typeof(JsonKnownTypeAttribute).Name);
            }

            return data;
        }

        private static void AddTypesWhichAreNotContains<T>(Dictionary<Type, string> typeToDiscriminator)
        {
            foreach (var heir in GetAllInheritance<T>())
            {
                if (!typeToDiscriminator.ContainsKey(heir))
                {
                    var name = DefaultNaming(heir.Name);
                    typeToDiscriminator.Add(heir, name);
                }
            }
        }

        private static IEnumerable<Type> GetAllInheritance<T>()
        {
            var type = typeof(T);
            return type.Assembly
                .GetTypes()
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        }
    }
}

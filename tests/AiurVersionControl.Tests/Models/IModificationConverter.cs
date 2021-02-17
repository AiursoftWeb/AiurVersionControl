using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiurVersionControl.Models;

namespace AiurVersionControl.Tests.Models
{
    public class IModificationConverter : JsonConverter<IModification<NumberWorkSpace>>
    {
        enum TypeDiscriminator
        {
            AddModification = 1
        }
        
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IModification<NumberWorkSpace>).IsAssignableFrom(typeToConvert);
        }

        public override IModification<NumberWorkSpace> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString();
            if (propertyName != "TypeDiscriminator")
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            IModification<NumberWorkSpace> value;
            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
            switch (typeDiscriminator)
            {
                case TypeDiscriminator.AddModification:
                    value = new AddModification();
                    break;

                default:
                    throw new JsonException();
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return value;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "Amount":
                            int creditLimit = reader.GetInt32();
                            ((AddModification)value).Amount = creditLimit;
                            break;
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IModification<NumberWorkSpace> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value is AddModification modification)
            {
                writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.AddModification);
                writer.WriteNumber("Amount", modification.Amount);
            }

            writer.WriteEndObject();
        }
    }
}
using Microsoft.Spatial;
using SurrealDb.Internals.Constants;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SurrealDb.Internals.Json.Converters.Spatial;

internal class GeographyMultiPointValueConverter : JsonConverter<GeographyMultiPoint>
{
	public override GeographyMultiPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.None || reader.TokenType == JsonTokenType.Null)
			return default;

		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		if (root.TryGetProperty(SpatialConverterConstants.TypePropertyName, out var typeProperty))
		{
			var type = typeProperty.GetString();

			if (type == MultiPointConverter.TypeValue)
			{
				var coordinatesProperty = root.GetProperty(SpatialConverterConstants.CoordinatesPropertyName);

				if (coordinatesProperty.ValueKind != JsonValueKind.Array)
					throw new JsonException($"Cannot deserialize {nameof(GeographyMultiPoint)} because coordinates must be an array");

				var geographyBuilder = GeographyFactory.MultiPoint();

				MultiPointConverter.ConstructGeographyMultiPoint(ref coordinatesProperty, geographyBuilder);

				return geographyBuilder.Build();
			}

			throw new JsonException($"Cannot deserialize {nameof(GeographyMultiPoint)} because of type \"{type}\"");
		}

		throw new JsonException($"Cannot deserialize {nameof(GeographyMultiPoint)}");
	}

	public override void Write(Utf8JsonWriter writer, GeographyMultiPoint value, JsonSerializerOptions options)
	{
		MultiPointConverter.WriteGeographyMultiPoint(writer, value);
	}
}

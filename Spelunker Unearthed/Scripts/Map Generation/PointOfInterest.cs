using System;
using MariEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public readonly struct PointOfInterest(PointOfInterestType poiType, Coord position)
{
    public PointOfInterestType PoiType { get; } = poiType;
    public Coord Position { get; } = position;
    
    public class YamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(PointOfInterest);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            parser.Consume<MappingStart>();

            parser.Consume<Scalar>();
            var poiType = Enum.Parse<PointOfInterestType>(parser.Consume<Scalar>().Value);

            parser.Consume<Scalar>();
            var position = new Coord(parser.Consume<Scalar>().Value);

            parser.Consume<MappingEnd>();

            return new PointOfInterest(poiType, position);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var poi = (PointOfInterest)value!;
            emitter.Emit(new MappingStart());
            
            emitter.Emit(new Scalar(nameof(poi.PoiType)));
            emitter.Emit(new Scalar(poi.PoiType.ToString()));
            emitter.Emit(new Scalar(nameof(poi.Position)));
            emitter.Emit(new Scalar(poi.Position.ToString()));
            
            emitter.Emit(new MappingEnd());
        }
    }
}
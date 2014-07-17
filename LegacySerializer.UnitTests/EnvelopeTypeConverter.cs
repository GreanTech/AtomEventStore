using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Grean.AtomEventStore.LegacySerializer.UnitTests
{
    internal class EnvelopeTypeConverter : TypeConverter
    {
        public override object ConvertTo(
            ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture,
            object value,
            Type destinationType)
        {
            var valueType = value.GetType();
            if (valueType.IsGenericType &&
                valueType.GetGenericTypeDefinition() == typeof(Envelope<>) &&
                destinationType.IsGenericType &&
                valueType.GetGenericTypeDefinition() == typeof(Envelope<>))
            {
                var sourceItemType = valueType.GetGenericArguments().Single();
                var destinationItemType = destinationType.GetGenericArguments().Single();
                var converterType = typeof(EnvelopeConverter<,>).MakeGenericType(sourceItemType, destinationItemType);
                dynamic converter = Activator.CreateInstance(converterType);
                return converter.Convert(value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private class EnvelopeConverter<TSource, TDestination>
        {
            public object Convert(object value)
            {
                return ((Envelope<TSource>)value).Cast<TDestination>();
            }
        }
    }
}

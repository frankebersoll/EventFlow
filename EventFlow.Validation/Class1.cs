using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;

namespace EventFlow.Validation
{
    public class ValidatingCommandBus : ICommandBus
    {
        private readonly ICommandBus _innerBus;

        public ValidatingCommandBus(ICommandBus innerBus)
        {
            _innerBus = innerBus;
        }

        public Task<TExecutionResult> PublishAsync<TAggregate, TIdentity, TExecutionResult>(
            ICommand<TAggregate, TIdentity, TExecutionResult> command, CancellationToken cancellationToken)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
            where TExecutionResult : IExecutionResult
        {
            return _innerBus.PublishAsync(command, cancellationToken);
        }
    }

    public static class ValidationManager
    {
        
        private static bool TryValidateObject(object obj, ICollection<ValidationResult> results)
        {
            return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
        }

        public static bool TryValidateObjectRecursive(object obj, out List<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            return TryValidateObjectRecursive(obj, results, new HashSet<object>());
        }

        private static readonly ConcurrentDictionary<Type, RecursiveProperty[]> PropertyMappings
            = new ConcurrentDictionary<Type, RecursiveProperty[]>();

        private class RecursiveProperty
        {
            private readonly PropertyInfo _propertyInfo;
            private readonly Func<object, object> _accessor;

            public RecursiveProperty(PropertyInfo propertyInfo)
            {
                _propertyInfo = propertyInfo;
                ParameterExpression o = Expression.Parameter(typeof(object), "o");
                Type propertyType = propertyInfo.PropertyType;
                _accessor = Expression.Lambda<Func<object, object>>(
                    Expression.Property(Expression.Convert(o, propertyType), propertyInfo),
                    o
                ).Compile();
            }

            public string Name => _propertyInfo.Name;

            public object Get(object o)
            {
                return _accessor(o);
            }
        }

        private static bool TryValidateObjectRecursive(object obj, ICollection<ValidationResult> results, ISet<object> validated)
        {
            validated.Add(obj);
            var result = TryValidateObject(obj, results);

            Type type = obj.GetType();
            var properties = PropertyMappings.GetOrAdd(type, CreatePropertyMapping);

            foreach (RecursiveProperty property in properties)
            {
                object value = property.Get(obj);

                switch (value)
                {
                    case null:
                    {
                        continue;
                    }

                    case IEnumerable enumerable:
                    {
                        foreach (object item in enumerable)
                        {
                            if (item == null) continue;
                        
                            result = TryValidateObjectRecursive(results, validated, value, property);
                        }

                        break;
                    }

                    default:
                    {
                        result = TryValidateObjectRecursive(results, validated, value, property);
                        break;
                    }
                }
            }

            return result;
        }

        private static bool TryValidateObjectRecursive(ICollection<ValidationResult> results, ISet<object> validated, object value,
            RecursiveProperty property)
        {
            if (validated.Contains(value))
                return true;

            var nestedResults = new List<ValidationResult>();
            if (TryValidateObjectRecursive(value, nestedResults, validated))
                return true;

            foreach (ValidationResult validationResult in nestedResults)
            {
                results.Add(new ValidationResult(validationResult.ErrorMessage,
                    validationResult.MemberNames.Select(x => property.Name + '.' + x)));
            }

            return false;
        }

        private static RecursiveProperty[] CreatePropertyMapping(Type type)
        {
            return type
                .GetProperties()
                .Where(p => CanRead(p) && CanValidateRecursive(p))
                .Select(p => new RecursiveProperty(p))
                .ToArray();
        }

        private static bool CanValidateRecursive(PropertyInfo property)
        {
            Type type = property.PropertyType;
            return type != typeof(string) && !type.IsValueType;
        }

        private static bool CanRead(PropertyInfo property)
        {
            return property.CanRead && property.GetIndexParameters().Length == 0;
        }
    }
}

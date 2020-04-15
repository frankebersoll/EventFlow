// The MIT License (MIT)
// 
// Copyright (c) 2015-2019 Rasmus Mikkelsen
// Copyright (c) 2015-2019 eBay Software Foundation
// https://github.com/eventflow/EventFlow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Threading;
using EventFlow;
using EventFlow.Aggregates;
using EventFlow.Commands;
using EventFlow.Configuration;
using EventFlow.Core;
using EventFlow.Extensions;
using EventFlow.Functional;
using NUnit.Framework;
using NUnitTestProject1;

namespace Tests
{
    public class CustomerId : Identity<CustomerId>
    {
        public CustomerId(string value) : base(value)
        {
        }
    }

    public enum CustomerState
    {
        Initial,
        Created,
        Deleted
    }

    public class CustomerAggregate : FnAggregateRoot<CustomerAggregate, CustomerId, CustomerState>
    {
        public CustomerAggregate(CustomerId id, IStateApplier stateApplier) : base(id, stateApplier)
        {
        }
    }

    public abstract class CustomerCommand : Command<CustomerAggregate, CustomerId>
    {
        protected CustomerCommand(CustomerId aggregateId) : base(aggregateId)
        {
        }
    }

    public abstract class CustomerEvent : IAggregateEvent<CustomerAggregate, CustomerId>
    {
    }

    public class CreateCustomer : CustomerCommand
    {
        public CreateCustomer(CustomerId aggregateId) : base(aggregateId)
        {
        }
    }

    public class CustomerCreated : CustomerEvent
    {
    }

    public class DeleteCustomer : CustomerCommand
    {
        public DeleteCustomer(CustomerId aggregateId) : base(aggregateId)
        {
        }
    }

    public class CustomerDeleted : CustomerEvent
    {
    }

    public class TestClass1
    {
        [Test]
        public void Test()
        {
            IEventFlowOptions eventFlow = EventFlowOptions.New
                .AddDefaults(typeof(TestClass1).Assembly);

            CustomerDefinition.Define(eventFlow);

            IRootResolver resolver = eventFlow.CreateResolver();
            ICommandBus bus = resolver.Resolve<ICommandBus>();

            CustomerId aggregateId = CustomerId.New;
            bus.PublishAsync(new CreateCustomer(aggregateId), CancellationToken.None);
            bus.PublishAsync(new DeleteCustomer(aggregateId), CancellationToken.None);
        }
    }
}

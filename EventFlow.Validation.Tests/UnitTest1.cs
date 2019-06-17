using System.ComponentModel.DataAnnotations;
using EventFlow.Validation;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var c = new TestCommand();
            ValidationManager.TryValidateObjectRecursive(c, out var results);
            Validator.ValidateObject();
        }

        public class TestCommand
        {
            [Required]
            public string Blah { get; }
        }
    }
}
using System;
using JukeboxAlexa.Library.Tests;

namespace JukeboxAlexa.Localstack
{
    class Program
    {
        static void Main(string[] args) {
            var localstackDynamoDbFixture = new LocalstackDynamoDb();
            var localstackSqsFixture = new LocalstackSqs();
            Console.WriteLine("Localstack Setup Complete");
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.SQS;
using Amazon.SQS.Model;
using JukeboxAlexa.Library;
using JukeboxAlexa.Library.Model;

namespace JukeboxAlexa.Library.TestFixture {
    public class AIntentRequestFixture : AIntentRequest {
        public AIntentRequestFixture(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient) : base(provider, awsSqsClient, "http://foo-bar") { }
        public override bool IsValidRequest() {
            return true;
        }

        public override string GenerateMessage() {
            throw new System.NotImplementedException();
        }

        public override void GetSongInfoRequested(Dictionary<string, Slot> intentSlots) {
            throw new System.NotImplementedException();
        }

        public override Task<CustomSkillResponse> HandleRequest(CustomSkillRequest customSkillRequest) {
            throw new System.NotImplementedException();
        }
    }
    
    public class AIntentRequestFixtureNotValidRequest : AIntentRequest {
        public AIntentRequestFixtureNotValidRequest(ICommonDependencyProvider provider, IAmazonSQS awsSqsClient) : base(provider, awsSqsClient, "http://foo-bar") { }
        public override bool IsValidRequest() {
            return false;
        }

        public override string GenerateMessage() {
            throw new System.NotImplementedException();
        }

        public override void GetSongInfoRequested(Dictionary<string, Slot> intentSlots) {
            throw new System.NotImplementedException();
        }

        public override Task<CustomSkillResponse> HandleRequest(CustomSkillRequest customSkillRequest) {
            throw new System.NotImplementedException();
        }
    }
}
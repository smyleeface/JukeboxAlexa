import botocore.session
import botocore.response
from botocore.stub import Stubber


class GenerateStubber(object):

    @staticmethod
    def client(client_type: str, response: dict, expected_params: dict, method: str):
        stubber_client = botocore.session.get_session().create_client(client_type)
        stubber = Stubber(stubber_client)
        stubber.add_response(method, response, expected_params)
        stubber.activate()
        return stubber_client

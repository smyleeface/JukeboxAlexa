Alexa Skill Configuration
=========================

- [Generate Skill JSON](#generate-skill-json)
- [Create Skill](#create-skill)
- [Update Skill](#update-skill)

## Generate Skill JSON

- [Download and install Docker](https://www.docker.com/community-edition)
- Update `custom_type_ARTISTS.txt` & `custom_type_SONGTITLE.txt` with updated lists.
- Run the commands below in `terminal` using `bash`.

```bash
    cd alexa_skill_configuration
    docker pull python:3.6-slim
    docker run --rm -it -v $PWD:/usr/src/app python:3.6-slim python /usr/src/app/generate_skill.py
```
- The `alexa_skill_configuration` directory will contain a new file called `skill.json`.

## Configure Skill
On the [Alexa developer portal](https://developer.amazon.com):

1. Create a new Alexa Skill called `jukebox`
1. Make the *Invocation Name*: `jukebox` (or whatever you want)
1. Save. Copy the Skill Id (aka Application Id) and use it in the CloudFormation parameters.
1. At this point you will need to run the jukebox CloudFormation to generate the infrastructure.
1. After the infrastructure is created, back on the Alexa developer portal for this project, in the `Configuration` tab, in the endpoint, add the Lambda ARN of the `JukeboxAlexa` Skill.
> Get the value of the `JukeboxAlexaLambdaArn` found in the [CloudFormation output](https://us-west-2.console.aws.amazon.com/cloudformation/home) of `cloudformation/jukebox-lambda.yaml`

## Update Skill
- In the `Skill Builder > Code Editor` section:
    - Copy/paste the generated `skill.json` 
     
       OR

       Drag and drop the `skill.json` file
- Test the skill in the `Test` section.

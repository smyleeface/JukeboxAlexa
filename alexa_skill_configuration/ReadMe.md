Alexa Skill Configuration
=========================

On the [Alexa developer portal](https://developer.amazon.com):

1. Create a new Alexa Skill called `jukebox`
1. Make the *Invocation Name*: `jukebox` (or whatever you want)
1. In the `Skill Builder > Code Editor` section:
    1. Copy/paste the generated `skill.json` 
     
       OR

       Drag and drop the `skill.json` file
1. On the `Configuration` tab, in the endpoint, add the Lambda ARN of the `JukeboxAlexa` Skill.
> The Lambda ARN can be found in the output of cloudformation/jukebox-lambda.yaml
1. Test the skill in the `Test` section.

## Generate Skill JSON

```bash

docker run --rm -it -v $PWD:/app python:3.6.4-slim python /app/generate_skill.py
```

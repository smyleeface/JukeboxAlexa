## Dockerfiles Used in CI/CD

* Publish dockerfile images
    * 
    ```bash
    cd jukebox_alexa
    bash docker/dockerfile-publish.sh default 123456789
    ```
* Use dockerfile images
    * dotnet
    ```bash
    cd jukebox_alexa
    docker run --rm -it -v ${HOME}/.aws:/root/.aws -v ${PWD}:/project jukebox_alexa_dotnet /bin/bash 

    ```
    * Python
    ```bash
    cd jukebox_alexa
    docker run --rm -it -v ${HOME}/.aws:/root/.aws -v ${PWD}:/project jukebox_alexa_python /bin/bash
    ```
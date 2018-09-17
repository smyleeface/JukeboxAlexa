## Setup Steps
* Clone the `LambdaSharp` Repo (0.2-rc1)
* From the `LambdaSharp` clone, go into `LambdaSharpTool`
* `dotnet tool install -g MindTouch.LambdaSharp.Tool  --version 0.2.0-pre`
* From the `LambdaSharp` clone, go into `LambdaSharpTool/Bootstrap/LambdaSharp`
```bash

lash deploy --tier devel --bootstrap --profile profilename
```
* Install `MindTouch.LambdaSharp` from NuGet into your project


## Troubleshooting

`ERROR: No RegionEndpoint or ServiceURL configured`
* aws credentials must include `region`

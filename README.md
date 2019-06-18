# Agile Visualizations in Immersive Augmented Reality

## How to Install AVAR
### Install Unity and Visual Studio for Mixed Reality Development
1. Install Mixed Reality Tools
    Follow the next Microsoft manual: [https://docs.microsoft.com/en-us/windows/mixed-reality/install-the-tools](https://docs.microsoft.com/en-us/windows/mixed-reality/install-the-tools). 
    Note: We used Unity (v 2018.3.11f1), Microsoft Visual Studio Community 2017 (Version 15.9.10), and Microsoft .NET Framework (Version 4.7.03190).

2. Download AVAR project
    Clone or download next git repository: https://github.com/bsotomayor92/AVAR-unity 

    ```
    $ git clone https://github.com/bsotomayor92/AVAR-unity.git
    ```

    Note: Over this folder you will have both Unity and Visual Studio projects.
3. Open you the project on Visual Studio:
        ⋅⋅* Visual Studio will find individual components which should be on the project. If not, they will be installed. This operation may require your computer to restart.

        ⋅⋅* Once you open Visual Studio for first time it will generate source files and build the project.

4. Open your the project on Unity.
        Open Unity, click on "Open a project folder" and select the folder downloaded from GitHub ("AVAR-unity").

       [//]: # (4.1. Player preferences settings ???)
       
       [//]: # (4.2. Compile and Deploy AVAR app on Hololens ???)
            
## Install Woden on Pharo 7.0

1. Installing Pharo Launcher:
    ⋅⋅* Go to https://pharo.org/download. Select your OS and download that version of Pharo Roassal. You can follow the installation documentation for each OS distribution.
2. Create an empty Virtual Machine:
        Create a virtual machine (VM) using Pharo Launcher by selecting Pharo 7.0 - 64bit (stable).

3. Download Woden
    ⋅⋅* Open a Playground window (Ctrl + O + W) and type the next:
    ```
    Metacello new
       baseline: 'WodenEngine';
       repository: 'github://ronsaldo/woden/tonel';
       load.
    ```
    Then, execute the script by pressing "Ctrl + D" ("DoIt"). This will download Woden Roassal Engine from the github repository.

4. Download AVAR changes
    ⋅⋅1. Open Monticello Browser by pressing "Ctrl + O +B"
    ⋅⋅2. On the next input field, search "WodenEngine-Roassal" and select it
    ⋅⋅3. Click on "+Repository" button
        ⋅⋅1. Select "smalltalkhub.com" option and complete as follows:
        ```
        MCSmalltalkhubRepository
            owner: 'Boris'
            project: 'AVAR_test'
            user: 'Boris'
            password: ''
        ```
        ⋅⋅2. Select "WodenEngine-Roassal" on the Right Panel and click on "Load" button. This will activate a procedure which overwrite archives.


5. Testing
    In order to verify if both repositories have been correctly installed copy the next example and paste it over a Pharo playground panel:
    ```smalltalk
     v := RWView new.
    els:= RWCylinder new color: Color blue; elementsOn: (1 to: 2).
    v addAll: els.
    RWXZGridLayout on: els.
    v encodeAllElementsInSceneAsJSON
    ```
    Next, select all the code and press "Ctrl + P" ("Print"). This will show a JSON representation of the view and its elements. In this case, the JSON corresponds to:
    ```json
        {"elements":[{
        "position":[0.0,0.0,5.0],
        "type":"camera"
        },{
        "shape":{
        "color":[0.0,0.0,1.0,1.0],
        "shapeDescription":"cylinder",
        "extent":[1.0,1.0,1.0]
        },
        "position":[1.0,0.0,0.0],
        "type":"element"
        },{
        "shape":{
        "color":[0.0,0.0,1.0,1.0],
        "shapeDescription":"cylinder",
        "extent":[1.0,1.0,1.0]
        },
        "position":[-1.0,0.0,0.0],
        "type":"element"
        }]}
    ```

## Running AVAR

1. Create a Pharo Service:
    ⋅⋅1. Open your Pharo Image, open a playground window, and then execute next code:
    ```smalltalk
    (ZnServer startDefaultOn: 1702)
        onRequestRespond: [ :request |
            script := (request contents).
            ZnResponse ok: (ZnEntity text: ( [ZnReadEvalPrintDelegate evaluate: script ] on: Error do: [:e | self halt. e asString, '. Message: ',e messageText asString, '. Location: ',e location asString.])) ].
    ```
    This will create a Service which evaluates and sends responses to unity application on Hololens. This response contains geometrical description of each object created in Pharo Roassal and Woden Engine.

2. Use the Unity App
    ⋅⋅1. Open the Unity project. In the "Hierarchy view, select the GameObject "PlayGroundManager".
    ⋅⋅2. Using the "Inspector" view, change the attributes "IP" and "Port" according to your configuration. If you are running Pharo and Unity on the same computer, then the IP corresponds to [http://127.0.0.1/](http://127.0.0.1). In this example, the Port indicated was "1702".

3. Using HoloLens:
    ⋅⋅1. On Hololens:
        ⋅⋅1. Install (if it is needed) the "Holographic Remoting Player" from Microsoft web store (direct link). Once installed, open it by tapping in the application icon.
        ⋅⋅2. The app shows the Hololens IP. This will be necessary in order to connect Unity and the Hololens.
    ⋅⋅2. On Unity GUI:
        ⋅⋅1. Click in "Holographic" Tab
        ⋅⋅2. Select "Remote to Device"
        ⋅⋅3. Insert the Remote Machine IP indicated in "Holographic Remoting Player" and click on "Connect" button.



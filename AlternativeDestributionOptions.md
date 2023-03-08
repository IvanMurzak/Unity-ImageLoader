# NPMJS
![image](https://user-images.githubusercontent.com/9135028/198755166-5d0f50a7-33e1-4c18-9462-ed880d099908.png)

NPMJS is the most popular Package destribution portal in the world. It is used for any packages from different programming areas of knowledge. We are interesting in using the platform for having dependencies on Unity packages only and publishing our own. It is free to use and work very well for my opinion.
### Pros
- Ultra fast deployment
- Easy creation of new versions
- Unity Package Manager supports versioning from NPMJS
- Trusted platform by huge community
### Cons
- Need to create account and authorize once

<br/><br/>

# OpenUPM
![image](https://user-images.githubusercontent.com/9135028/198767467-993b7b46-7d5f-440a-a15e-2d7c7b968bcb.png)

Popular in Unity community platform for package destribution. Created as open sourced project for helping people to destribute their packages.
### Pros
- Made especially for Unity
- No registration needed
- Package can be deployed directly from GitHub repository
### Cons
- Long deployment duration (10 - 60 minutes)
- Small community (in comparison to other options)

<br/><br/>

# GitHub Packages
![image](https://user-images.githubusercontent.com/9135028/198767290-688cf8eb-a350-40c4-beb6-a50dcbe536a6.png)

Amazing GitHub feature, also Unity support it
### Pros
- Ultra fast deployment 
- Trusted platform by huge community
### Cons
- Does not support version fetching, it means you can't see when new version of the package is available in UPM. To install new version you should manually change the version in a project.

<br/><br/>

# GitHub Repository
![image](https://user-images.githubusercontent.com/9135028/198767290-688cf8eb-a350-40c4-beb6-a50dcbe536a6.png)

Unity UPM support direct GitHub links to public repositories for using them as a package. The only required thing - the link should point on a folder which contains `package.json` file.
### Pros
- no special steps required, just use your public repository as a package in UPM
### Cons
- Does not support version fetching, it means you can't see when new version of the package is available in UPM. To install new version you should manually change the version in a project.

<br/><br/>

# How to use
- "Use this template" green button at top right corner of GitHub page
- Clone your new repository
- Add all your stuff to <code>Assets/_PackageRoot directory</code>
- Update <code>Assets/_PackageRoot/package.json</code> to yours
- (on Windows) execute <code>gitSubTreePushToUPM.bat</code>
- (on Mac) execute <code>gitSubTreePushToUPM.makefile</code>

- (optional) Create release from UPM branch on GitHub web page for support different versions

![alt text](https://neogeek.dev/images/creating-custom-packages-for-unity-2018.3--git-release.png)


# How to import your package to Unity project
You may use one of the variants

## Variant 1
- Select "Add package from git URL"
- Paste URL to your GitHub repository with simple modification:
- <code>https://github.com/USER/REPO.git#upm</code> 
Dont forget to replace **USER** and **REPO** to yours

![alt text](https://neogeek.dev/images/creating-custom-packages-for-unity-2018.3--package-manager.png)

### **Or** you may use special version if you create one  
<code>https://github.com/USER/REPO.git#v1.0.0</code>
Dont forget to replace **USER** and **REPO** to yours

## Variant 2
Modify manifest.json file. Change <code>"your.own.package"</code> to the name of your package.
Dont forget to replace **USER** and **REPO** to yours.
<pre><code>{
    "dependencies": {
        "your.own.package": "https://github.com/USER/REPO.git#upm"
    }
}
</code></pre>

### **Or** you may use special version if you create one
Dont forget to replace **USER** and **REPO** to yours.
<pre><code>{
    "dependencies": {
        "your.own.package": "https://github.com/USER/REPO.git#v1.0.0"
    }
}
</code></pre>

# Gamejam Template project

Base Unity project to generate projects off of. It will contain boilerplate code that we find we will be needing for future gamejams that we may participate in, and help us in the future to actually focus on proper game implementation. 

The idea is to have this project up to date and functional with the latest Unity LTS version (if possible), and use GitHub's template repository feature to create new repos off of it, so we can be up to speed quickly.

Anyone willing to make improvements for future years (or even add stuff that we've added specifically to a game that would be good for future games), feel free to make a pull request! Any improvements to this base will mean time saved during the actual gamejam.

## Project contents
- Basic UI/Menu system
- FMOD for Unity installed and set up with a test bank
- ...

## Setting up FMOD for Unity
When first setting up the project (either when updating the template, or as a new repository created off of it) you will be greeted by the FMOD for Unity integration setup wizard. Please go through its steps for the Unity project to function correctly. The wizard is very straight forward, but when you reach the "Linking" step, you will need to tell the wizard where to look for the built FMOD bank.

For this, you will want to click on "Single Platform Build", then on the window that pops up, you will want to pick the following directory:

```
/your/path/to/your/project/Assets/FMOD/Desktop
```

This should indicate FMOD where to find the banks, and everything should link properly. Proceed through the setup wizard by pressing Next until you reach the last step and the wizard is completed.
// On WebGL builds, `OnApplicationFocus` doesn't trigger when switching tabs/minimizing the web browser.
// This JS plugin should let us listen for an event in the web browser if the tab is or isn't in focus at the moment.
// https://forum.unity.com/threads/onpause-events-for-webgl-builds.432775/#post-2877495
const visibilityChangePlugin = {
     registerVisibilityChangeEvent: function () {
         document.addEventListener("visibilitychange", function () {
             MyGameInstance.SendMessage("CanvasMainMenuUi", "OnVisibilityChange", document.visibilityState);
             MyGameInstance.SendMessage("CanvasGameUi", "OnVisibilityChange", document.visibilityState);
         });
         if (document.visibilityState != "visible") {
             MyGameInstance.SendMessage("CanvasMainMenuUi", "OnVisibilityChange", document.visibilityState);
             MyGameInstance.SendMessage("CanvasGameUi", "OnVisibilityChange", document.visibilityState);
         }
     },
};

mergeInto(LibraryManager.library, visibilityChangePlugin);
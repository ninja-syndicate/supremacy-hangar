// Read more about creating JS plugins: https://www.patrykgalach.com/2020/04/27/unity-js-plugin/

// Creating functions for the Unity
mergeInto(LibraryManager.library, {

   // Method used to send a message to the page
   SiloReady: function () {
      try {
        window.dispatchReactUnityEvent("SiloReady");
      } catch (e) {
        console.warn("Unable to dispatch");
      }
   },
   
   RequestCrateContent: function (ownership_id) {
      try {
        window.dispatchReactUnityEvent("RequestCrateContent", Pointer_stringify(ownership_id));
      } catch (e) {
        console.warn("Unable to dispatch RequestCrateContent");
      }
   }
});
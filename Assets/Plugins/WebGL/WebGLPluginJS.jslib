// Read more about creating JS plugins: https://www.patrykgalach.com/2020/04/27/unity-js-plugin/

// Creating functions for the Unity
mergeInto(LibraryManager.library, {

   // Method used to send a message to the page
   SiloReady: function () {
      // Pass message to the page
      SiloReady(); // This function is embeded into the page
   }
});
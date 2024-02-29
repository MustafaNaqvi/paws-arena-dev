mergeInto(LibraryManager.library, {
  CopyToClipboard: function(textPtr) {
    // Convert pointer to string
    var text = UTF8ToString(textPtr);

    // Modern approach with Clipboard API
    navigator.clipboard.writeText(text).then(function() {
      console.log('Copying to clipboard was successful!');
    }, function(err) {
      console.error('Could not copy text to clipboard: ', err);
    });
  }
});
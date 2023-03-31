mergeInto(LibraryManager.library, {
  IsMobileDevice: function () {
    return (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) ? 1 : 0;
  },
});

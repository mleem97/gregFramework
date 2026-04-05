(function () {
  try {
    var supported = ['en', 'de', 'fr', 'es', 'ru', 'ja'];
    var defaultLocale = 'en';
    var storageKey = 'frikamf-locale';
    var redirectMarkerKey = 'frikamf-locale-redirected';

    function pathLocale(pathname) {
      var first = pathname.split('/').filter(Boolean)[0];
      if (!first) {
        return defaultLocale;
      }
      return supported.indexOf(first) >= 0 ? first : defaultLocale;
    }

    function detectFromRegionAndLanguage() {
      var localeCandidates = [];
      if (Array.isArray(navigator.languages)) {
        localeCandidates = localeCandidates.concat(navigator.languages);
      }
      localeCandidates.push(navigator.language || '');
      localeCandidates.push(Intl.DateTimeFormat().resolvedOptions().locale || '');

      var combined = localeCandidates
        .filter(Boolean)
        .map(function (value) {
          return String(value).toLowerCase();
        })
        .join(',');

      var firstLocale = combined.split(',')[0] || '';

      var languagePart = firstLocale.split('-')[0];
      if (supported.indexOf(languagePart) >= 0) {
        return languagePart;
      }

      var regionPart = firstLocale.split('-')[1] || '';
      var regionMap = {
        de: 'de',
        at: 'de',
        ch: 'de',
        fr: 'fr',
        be: 'fr',
        ca: 'fr',
        es: 'es',
        mx: 'es',
        ar: 'es',
        cl: 'es',
        co: 'es',
        pe: 'es',
        ru: 'ru',
        by: 'ru',
        kz: 'ru',
        ua: 'ru',
        jp: 'ja',
      };

      return regionMap[regionPart] || defaultLocale;
    }

    var pathname = window.location.pathname;
    var currentLocale = pathLocale(pathname);

    if (currentLocale !== defaultLocale) {
      localStorage.setItem(storageKey, currentLocale);
      sessionStorage.setItem(redirectMarkerKey, '1');
      return;
    }

    var savedLocale = localStorage.getItem(storageKey);
    var targetLocale = savedLocale || detectFromRegionAndLanguage();

    if (supported.indexOf(targetLocale) < 0) {
      targetLocale = defaultLocale;
    }

    if (targetLocale === defaultLocale) {
      return;
    }

    var isOnRootLikePath = pathname === '/' || pathname === '';
    if (!isOnRootLikePath) {
      return;
    }

    if (sessionStorage.getItem(redirectMarkerKey) === '1') {
      return;
    }

    var redirectTarget = '/' + targetLocale + '/';
    if (window.location.pathname !== redirectTarget) {
      sessionStorage.setItem(redirectMarkerKey, '1');
      window.location.replace(redirectTarget + window.location.search + window.location.hash);
    }
  } catch (error) {
    // no-op
  }
})();

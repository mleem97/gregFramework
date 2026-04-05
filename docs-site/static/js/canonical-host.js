(function () {
  try {
    var allowedDomainRoots = [
      'gregframework.eu',
      'gregframework',
      'datacentermods.com',
      'datacentermods',
      'frikadellental.de',
    ];

    var currentHost = window.location.hostname.toLowerCase();
    var currentOrigin = window.location.origin;
    var path = window.location.pathname + window.location.search;

    function isAllowedHost(host, root) {
      return host === root || host.endsWith('.' + root);
    }

    var hostAllowed = allowedDomainRoots.some(function (root) {
      return isAllowedHost(currentHost, root);
    });

    var fallbackOrigin = 'https://frikadellental.de';
    var canonicalNode = document.querySelector('link[rel="canonical"]');
    if (canonicalNode) {
      try {
        fallbackOrigin = new URL(canonicalNode.getAttribute('href')).origin;
      } catch (error) {
        // keep default fallback
      }
    }

    var chosenOrigin = hostAllowed ? currentOrigin : fallbackOrigin;
    var currentUrl = chosenOrigin + path;

    if (canonicalNode) {
      canonicalNode.setAttribute('href', currentUrl);
    }

    var ogUrl = document.querySelector('meta[property="og:url"]');
    if (ogUrl) {
      ogUrl.setAttribute('content', currentUrl);
    }

    var alternates = document.querySelectorAll('link[rel="alternate"]');
    alternates.forEach(function (node) {
      var href = node.getAttribute('href');
      if (!href) {
        return;
      }

      try {
        var parsed = new URL(href);
        var normalized = chosenOrigin + parsed.pathname + parsed.search;
        node.setAttribute('href', normalized);
      } catch (error) {
        // ignore malformed URLs
      }
    });
  } catch (error) {
    // no-op
  }
})();

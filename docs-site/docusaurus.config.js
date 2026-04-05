// @ts-check

const config = {
  title: 'Frika Mod Framework',
  tagline: 'Community-driven docs for FrikaMF, plugins, Rust FFI, and multiplayer stacks',
  favicon: 'img/logo.svg',
  url: 'https://frikadellental.de',
  baseUrl: '/',
  onBrokenLinks: 'warn',
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  scripts: ['/js/canonical-host.js'],
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'de', 'fr', 'es', 'ru', 'ja'],
    localeConfigs: {
      en: {label: 'English', htmlLang: 'en-GB'},
      de: {label: 'Deutsch', htmlLang: 'de-DE'},
      fr: {label: 'Français', htmlLang: 'fr-FR'},
      es: {label: 'Español', htmlLang: 'es-ES'},
      ru: {label: 'Русский', htmlLang: 'ru-RU'},
      ja: {label: '日本語', htmlLang: 'ja-JP'},
    },
  },
  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          routeBasePath: '/',
          editUrl: 'https://github.com/mleem97/FrikaModFramework/tree/master/docs-site/',
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
  themes: ['@docusaurus/theme-live-codeblock'],
  plugins: [
    '@docusaurus/plugin-css-cascade-layers',
    [
      '@docusaurus/plugin-client-redirects',
      {
        redirects: [
          {
            to: '/mods/standalone',
            from: ['/mods', '/plugins', '/standalone-mods'],
          },
          {
            to: '/mods/framework',
            from: ['/framework'],
          },
        ],
      },
    ],
    function tailwindPlugin() {
      return {
        name: 'tailwindcss-integration',
        /** @param {{plugins: Array<unknown>}} postcssOptions */
        configurePostCss(postcssOptions) {
          postcssOptions.plugins.push(require('@tailwindcss/postcss'));
          return postcssOptions;
        },
      };
    },
  ],
  themeConfig: {
    image: 'img/logo.svg',
    colorMode: {
      defaultMode: 'dark',
      disableSwitch: true,
      respectPrefersColorScheme: false,
    },
    navbar: {
      title: 'Frika Mod Framework',
      hideOnScroll: false,
      style: 'dark',
      items: [
        {to: '/', label: 'Home', position: 'left'},
        {
          label: 'Docs Hub',
          position: 'left',
          items: [
            {to: '/docs', label: 'Overview'},
            {to: '/mods/framework', label: 'Framework'},
            {to: '/mods/standalone', label: 'Standalone (Plugins & Mods)'},
            {to: '/wiki-import/Framework-Features-Use-Cases', label: 'Ecosystem'},
          ],
        },
        {
          type: 'localeDropdown',
          className: 'nav-locale nav-right-icon nav-icon-only',
          position: 'right',
          dropdownItemsBefore: [],
          dropdownItemsAfter: [],
        },
        {to: '/mods/standalone', label: 'Mods', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-mods', 'aria-label': 'Mods'},
        {href: 'https://discord.gg/greg', label: 'Discord', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-discord', 'aria-label': 'Discord'},
        {href: 'https://github.com/mleem97/FrikaModFramework/issues', label: 'Support', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-support', 'aria-label': 'Support'},
        {href: 'https://github.com/mleem97/FrikaModFramework', label: 'GitHub', position: 'right', className: 'nav-right-icon nav-icon-only nav-link-github', 'aria-label': 'GitHub'},
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Community',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/mleem97/FrikaModFramework',
              className: 'footer-link-icon footer-link-github',
            },
            {
              label: 'Discord',
              href: 'https://discord.gg/greg',
              className: 'footer-link-icon footer-link-discord',
            },
            {
              label: 'Support',
              href: 'https://github.com/mleem97/FrikaModFramework/issues',
              className: 'footer-link-icon footer-link-support',
            },
          ],
        },
      ],
      copyright: `Copyright ${new Date().getFullYear()} <a href="https://meyermedia.eu" target="_blank" rel="noopener noreferrer">Meyer Media</a><br/>Dieses Wiki ist ein Community-Projekt und steht in keiner Verbindung zu WASEKU oder dem Spiel selbst.`,
    },
  },
};

module.exports = config;

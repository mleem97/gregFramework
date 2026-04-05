/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  docs: [
    'intro',
    {
      type: 'category',
      label: 'Mods',
      items: [
        'mods/framework',
        {
          type: 'category',
          label: 'Standalone Mods',
          items: [
            'mods/standalone/index',
            'mods/standalone/ffm-plugin-asset-exporter',
            'mods/standalone/ffm-plugin-multiplayer',
            'mods/standalone/ffm-plugin-player-models',
            'mods/standalone/ffm-plugin-sysadmin',
            'mods/standalone/ffm-plugin-web-ui-bridge',
            'mods/standalone/fmf-console-input-guard',
            'mods/standalone/fmf-gregify-employees',
            'mods/standalone/fmf-hex-label-mod',
            'mods/standalone/fmf-lang-compat-bridge',
            'mods/standalone/fmf-ui-replacement-mod',
          ],
        },
      ],
    },
    {
      type: 'category',
      label: 'By Experience',
      items: [
        'audiences/newbies',
        'audiences/intermediates',
        'audiences/professionals',
      ],
    },
    {
      type: 'category',
      label: 'Reference',
      items: [
        'reference/wiki-mapping',
        'reference/mod-store-vision',
      ],
    },
    {
      type: 'category',
      label: 'Contributors',
      items: [
        'contributors/docusaurus-workflow',
      ],
    },
    {
      type: 'category',
      label: 'Roadmap',
      items: ['roadmap/mod-store-stages'],
    },
  ],
};

module.exports = sidebars;

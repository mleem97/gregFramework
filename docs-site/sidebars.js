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
          label: 'Plugin Wiki',
          items: [
            'mods/plugins/index',
            'mods/plugins/ffm-plugin-asset-exporter',
            'mods/plugins/ffm-plugin-asset-exporter-release',
            'mods/plugins/ffm-plugin-multiplayer',
            'mods/plugins/ffm-plugin-multiplayer-release',
            'mods/plugins/ffm-plugin-player-models',
            'mods/plugins/ffm-plugin-player-models-release',
            'mods/plugins/ffm-plugin-sysadmin',
            'mods/plugins/ffm-plugin-sysadmin-release',
            'mods/plugins/ffm-plugin-web-ui-bridge',
            'mods/plugins/ffm-plugin-web-ui-bridge-release',
          ],
        },
        {
          type: 'category',
          label: 'Mod Wiki',
          items: [
            'mods/mods/index',
            'mods/mods/fmf-console-input-guard',
            'mods/mods/fmf-console-input-guard-release',
            'mods/mods/fmf-gregify-employees',
            'mods/mods/fmf-gregify-employees-release',
            'mods/mods/fmf-hex-label-mod',
            'mods/mods/fmf-hex-label-mod-release',
            'mods/mods/fmf-lang-compat-bridge',
            'mods/mods/fmf-lang-compat-bridge-release',
            'mods/mods/fmf-ui-replacement-mod',
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
        'contributors/luminescent-design-system',
        'contributors/docusaurus-workflow',
        'contributors/plugin-submission-audit',
      ],
    },
    {
      type: 'category',
      label: 'Roadmap',
      items: ['roadmap/unified-roadmap', 'roadmap/mod-store-stages'],
    },
  ],
};

module.exports = sidebars;

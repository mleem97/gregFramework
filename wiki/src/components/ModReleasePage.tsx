import React from 'react';
import Link from '@docusaurus/Link';
import Translate from '@docusaurus/Translate';

type ReleaseKind = 'mod' | 'plugin';

type ModReleasePageProps = {
  title: string;
  kind: ReleaseKind;
  dllName: string;
  description: string;
  version?: string;
  author?: string;
  category?: string;
  dependencies?: string[];
  codeLanguages?: string[];
  releaseReady?: boolean;
  banner?: string;
  releaseNotesPath?: string;
  /** If set, replaces the default installation steps (e.g. MelonLoader `Plugins/` vs `Mods/`). */
  installation?: React.ReactNode;
};

function toTitleCase(value: ReleaseKind): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export default function ModReleasePage({
  title,
  kind,
  dllName,
  description,
  version = 'NotReleasedYet',
  author = 'FrikaMF Community',
  category = 'Mod',
  dependencies = [],
  codeLanguages = ['C#'],
  releaseReady = false,
  banner,
  releaseNotesPath,
  installation,
}: ModReleasePageProps): JSX.Element {
  const downloadPath = `/${kind}/${dllName}`;
  const downloadUrl =
    typeof window !== 'undefined' ? new URL(downloadPath, window.location.origin).toString() : downloadPath;
  const normalizedReleaseNotesPath = releaseNotesPath?.startsWith('/mods/')
    ? `/wiki${releaseNotesPath}`
    : releaseNotesPath;

  return (
    <div className="mod-release-page">
      <section className="mod-release-hero">
        <p className="mod-release-badge">{toTitleCase(kind)}</p>
        <h1>{title}</h1>
        <p>
          {banner ?? (
            <Translate id="modRelease.heroDefaultBanner">Official release download and module details</Translate>
          )}
        </p>
      </section>

      <section className="mod-release-layout">
        <article className="mod-release-content">
          <h2>
            <Translate id="modRelease.descriptionTitle">Description</Translate>
          </h2>
          <p>{description}</p>

          <h2>
            <Translate id="modRelease.installationTitle">Installation</Translate>
          </h2>
          {installation ?? (
            <ol>
              <li>
                <Translate id="modRelease.installStepOne">Download the release DLL.</Translate>
              </li>
              <li>
                <Translate id="modRelease.installStepTwo">Copy it to your Data Center `Mods/` folder.</Translate>
              </li>
              <li>
                <Translate id="modRelease.installStepThree">Start the game with MelonLoader.</Translate>
              </li>
            </ol>
          )}

          <h2>
            <Translate id="modRelease.dllRouteTitle">Direct DLL Route</Translate>
          </h2>
          <p>
            <code>{downloadPath}</code>
          </p>
        </article>

        <aside className="mod-release-sidebar" aria-label="Download Sidebar">
          <div className="mod-release-sidebar-card">
            <h3>
              <Translate id="modRelease.sidebarTitle">Download</Translate>
            </h3>

            {releaseReady ? (
              <a className="button button--primary button--lg mod-release-download-btn" href={downloadUrl}>
                <Translate id="modRelease.downloadButton">Download DLL</Translate>
              </a>
            ) : (
              <button className="button button--primary button--lg mod-release-download-btn" disabled type="button">
                <Translate id="modRelease.notReleasedYet">NotReleasedYet</Translate>
              </button>
            )}

            {normalizedReleaseNotesPath ? (
              <Link className="button button--secondary button--sm mod-release-secondary-btn" to={normalizedReleaseNotesPath}>
                <Translate id="modRelease.releaseNotesButton">Open module docs</Translate>
              </Link>
            ) : null}

            <dl className="mod-release-meta-list">
              <div>
                <dt>
                  <Translate id="modRelease.metaVersion">Version</Translate>
                </dt>
                <dd>{version}</dd>
              </div>
              <div>
                <dt>
                  <Translate id="modRelease.metaAuthor">Author</Translate>
                </dt>
                <dd>{author}</dd>
              </div>
              <div>
                <dt>
                  <Translate id="modRelease.metaCategory">Category</Translate>
                </dt>
                <dd>{category}</dd>
              </div>
              <div>
                <dt>
                  <Translate id="modRelease.metaDependencies">Dependencies</Translate>
                </dt>
                <dd>{dependencies.join(', ')}</dd>
              </div>
              <div>
                <dt>
                  <Translate id="modRelease.metaCodeLanguage">Code Language</Translate>
                </dt>
                <dd>{codeLanguages.join(', ')}</dd>
              </div>
            </dl>

            <p className="mod-release-sidebar-note">
              <Translate id="modRelease.sidebarHint">
                The download route redirects to the latest GitHub release asset.
              </Translate>
            </p>
          </div>
        </aside>
      </section>
    </div>
  );
}

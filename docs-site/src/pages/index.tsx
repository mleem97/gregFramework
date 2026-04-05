import React, { useEffect } from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import gregImage from '../image.png';
import { getHomepageContent } from '../i18n/homepage';
import {
  FaArrowUpRightFromSquare,
  FaBookOpen,
  FaCode,
  FaDiscord,
  FaGithub,
  FaLifeRing,
  FaPeopleGroup,
  FaShop,
  FaScrewdriverWrench,
  FaServer,
} from 'react-icons/fa6';

type FeatureItem = {
  icon: React.ReactNode;
  title: string;
  description: string;
};

function useScrollAnimations() {
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries, obs) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) {
            return;
          }

          entry.target.classList.remove('fade-in-hidden');
          entry.target.classList.add('fade-in-visible');
          obs.unobserve(entry.target);
        });
      },
      { threshold: 0.15 },
    );

    const animated = document.querySelectorAll('.animate-on-scroll');
    animated.forEach((node) => observer.observe(node));

    return () => observer.disconnect();
  }, []);
}

export default function HomePage(): JSX.Element {
  const {
    i18n: { currentLocale },
  } = useDocusaurusContext();

  const t = getHomepageContent(currentLocale);

  const features: FeatureItem[] = [
    {
      icon: <FaScrewdriverWrench className="text-xl" />,
      title: t.featureTitles[0],
      description: t.featureDescriptions[0],
    },
    {
      icon: <FaCode className="text-xl" />,
      title: t.featureTitles[1],
      description: t.featureDescriptions[1],
    },
    {
      icon: <FaBookOpen className="text-xl" />,
      title: t.featureTitles[2],
      description: t.featureDescriptions[2],
    },
    {
      icon: <FaPeopleGroup className="text-xl" />,
      title: t.featureTitles[3],
      description: t.featureDescriptions[3],
    },
  ];

  useScrollAnimations();

  return (
    <Layout
      title="Frika Mod Framework"
      description="Community docs for FrikaMF, standalone Rust stacks, multiplayer, and plugins.">
      <main style={{ backgroundColor: 'var(--color-app-bg)' }}>
        {/* Hero Section */}
        <section className="flex min-h-[60vh] flex-col items-center justify-center px-4 py-20 text-center">
          <h1 className="homepage-logo-title mb-8 text-3xl font-black text-white md:text-5xl leading-none">
            FRIKA MOD <span className="text-amber-700">🍪</span>
            <br />
            FRAMEWORK
          </h1>

          <h2 className="mb-4 fade-in-hidden animate-on-scroll text-2xl font-extrabold text-white md:text-4xl tracking-tight" style={{ transitionDelay: '100ms' }}>
            {t.heroLine1}
            <br />
            <span className="text-gray-400">{t.heroLine2}</span>
          </h2>

          <p className="mb-10 fade-in-hidden animate-on-scroll max-w-lg text-base font-medium text-gray-300 md:text-lg" style={{ transitionDelay: '200ms' }}>
            {t.heroSub1}
            <br />
            {t.heroSub2}
          </p>

          <div className="flex flex-wrap items-center justify-center gap-3 fade-in-hidden animate-on-scroll" style={{ transitionDelay: '300ms' }}>
            <Link to="/mods/framework" className="app-btn-primary px-8 py-4 rounded-full text-lg font-bold shadow-lg transition-all hover:scale-105">
              {t.ctaStart}
            </Link>
            <Link to="/mods/standalone" className="app-card px-8 py-4 rounded-full text-lg font-bold border transition-all hover:border-opacity-100" style={{ borderColor: 'var(--color-nav-pill)' }}>
              {t.ctaMods}
            </Link>
          </div>
        </section>

        {/* Features Grid */}
        <section id="features" className="border-t px-4 py-20" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-app-bg)' }}>
          <div className="mx-auto grid max-w-6xl grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            {features.map((feature, index) => (
              <article
                key={feature.title}
                className="app-card fade-in-hidden animate-on-scroll p-5 rounded-xl text-gray-200 transition-colors"
                style={{ transitionDelay: `${index * 100}ms` }}>
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  {feature.icon}
                  <span>{feature.title}</span>
                </h3>
                <p className="text-sm font-medium text-gray-400">{feature.description}</p>
              </article>
            ))}
          </div>
        </section>

        {/* Documentation Paths Section */}
        <section id="docs" className="border-t px-4 py-20" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-app-bg)' }}>
          <div className="mx-auto max-w-5xl text-center">
            <h2 className="mb-10 fade-in-hidden animate-on-scroll text-3xl font-bold text-white">{t.docsPaths}</h2>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Link to="/wiki-import/End-User-Release" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">End-User</div>
                <div className="text-sm text-gray-400">Install, update, FAQ, troubleshooting.</div>
              </Link>
              <Link to="/wiki-import/Mod-Developer-Debug" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">Mod-Devs</div>
                <div className="text-sm text-gray-400">Debug workflows, setup, hooks and references.</div>
              </Link>
              <Link to="/wiki-import/Contirbutors/Contributors-Debug" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">Contributors</div>
                <div className="text-sm text-gray-400">Conventions, CI checks, contribution workflow.</div>
              </Link>
              <Link to="/wiki-import/Framework-Features-Use-Cases" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">Capability Matrix</div>
                <div className="text-sm text-gray-400">Complete feature map and implementation use cases.</div>
              </Link>
            </div>
          </div>
        </section>

        <section id="ecosystem" className="border-t border-brandPink-900 bg-brandDark px-4 py-20">
          <div className="mx-auto max-w-6xl">
            <h2 className="mb-8 text-center text-4xl font-bold text-white">Ecosystem Coverage</h2>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              <article className="rounded-xl border border-brandPink-900 bg-gray-900 p-6">
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaCode />
                  FrikaMF Core
                </h3>
                <p className="text-sm text-gray-400">Hook system, FFI bridge, event contracts and runtime architecture.</p>
                <Link to="/wiki-import/Framework-Features-Use-Cases" className="mt-3 inline-block text-sm font-semibold text-brandPink-500 hover:text-brandPink-300">
                  Open core docs →
                </Link>
              </article>

              <article className="rounded-xl border border-brandPink-900 bg-gray-900 p-6">
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaServer />
                  Standalone Rust Frameworks
                </h3>
                <p className="text-sm text-gray-400">Rust/FFI implementation guides for external and standalone runtimes.</p>
                <Link to="/wiki-import/Lua-FFI-Start-Developing" className="mt-3 inline-block text-sm font-semibold text-brandPink-500 hover:text-brandPink-300">
                  Open Rust/FFI docs →
                </Link>
              </article>

              <article className="rounded-xl border border-brandPink-900 bg-gray-900 p-6">
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaPeopleGroup />
                  Multiplayer & Plugins
                </h3>
                <p className="text-sm text-gray-400">Multiplayer roadmap, plugin docs, and community module references.</p>
                <Link to="/wiki-import/Steamworks-P2P-Multiplayer-Roadmap" className="mt-3 inline-block text-sm font-semibold text-brandPink-500 hover:text-brandPink-300">
                  Open multiplayer docs →
                </Link>
              </article>
            </div>
          </div>
        </section>

        <section id="greg-story" className="border-t border-brandPink-900 bg-brandDark px-4 py-20">
          <div className="mx-auto grid max-w-6xl grid-cols-1 items-center gap-8 rounded-2xl border border-brandPink-900 bg-gray-900 p-6 md:grid-cols-[360px_1fr] md:p-8">
            <div className="mx-auto w-full max-w-90 overflow-hidden rounded-xl border border-brandPink-900 bg-black/30 shadow-2xl">
              <img src={gregImage} alt="Greg, server technician" className="h-auto w-full object-cover" />
            </div>

            <div>
              <h2 className="mb-4 text-3xl font-extrabold text-white md:text-4xl">Die Legende von Greg</h2>
              <p className="mb-4 text-gray-300">
                Greg ist der unermüdliche Server-Techniker des Projekts. Er wirkt zwar so, als hätte er seit Monaten nicht mehr
                geschlafen, und er ist meistens still — aber sobald ein Rack ausfällt oder ein Mod zickt, ist Greg schon da.
              </p>
              <p className="mb-4 text-gray-300">
                Unterwürfig erfüllt er all deine Wünsche: mehr Uptime, sauberere Configs, bessere Logs und weniger Drama im
                Deployment. Er fragt nicht viel, er liefert einfach. Und wenn es um Open Source und Server geht, schlägt sein Herz
                lauter als jeder Lüfter im Datacenter.
              </p>
              <p className="inline-block rounded-lg border border-brandPink-800 bg-brandPink-900/40 px-4 py-2 text-lg font-bold text-white">
                “Be smart. Be like Greg.”
              </p>
            </div>
          </div>
        </section>

        <section id="community" className="border-t border-brandPink-900 bg-brandDark px-4 py-16">
          <div className="mx-auto mb-6 max-w-6xl rounded-xl border border-amber-500/40 bg-amber-950/30 p-4 text-amber-200">
            <div className="text-sm font-semibold uppercase tracking-wide">{t.comingSoon}</div>
            <div className="mt-1 text-base font-medium">
              {t.comingSoonText}
            </div>
          </div>

          <div className="mx-auto flex max-w-6xl flex-col items-start justify-between gap-6 rounded-xl border border-brandPink-900 bg-gray-900 p-6 md:flex-row md:items-center">
            <div>
              <h3 className="text-2xl font-bold text-white">{t.communityTitle}</h3>
              <p className="mt-2 text-gray-400">
                {t.communityText}
              </p>
            </div>
            <div className="flex flex-wrap gap-3">
              <Link to="https://frikadellental.de" className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
                <FaArrowUpRightFromSquare />
                frikadellental.de
              </Link>
              <Link to="/mods/standalone" className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
                <FaShop />
                Available Mods
              </Link>
              <Link to="https://github.com/mleem97/FrikaModFramework" className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
                <FaGithub />
                Repository
              </Link>
              <Link to="https://github.com/mleem97/FrikaModFramework" className="inline-flex items-center gap-2 rounded border border-gray-700 bg-gray-800 px-4 py-2 font-bold text-white transition-colors hover:border-gray-500 hover:bg-gray-700">
                <FaArrowUpRightFromSquare />
                PROJECT
              </Link>
              <Link to="https://discord.gg/greg" className="inline-flex items-center gap-2 rounded bg-[#5865F2] px-4 py-2 font-bold text-white transition-colors hover:bg-[#4752c4]">
                <FaDiscord />
                JOIN
              </Link>
            </div>
          </div>
        </section>

        <section id="support" className="border-t border-brandPink-900 bg-brandDark px-4 py-16">
          <div className="mx-auto flex max-w-6xl flex-col items-start justify-between gap-4 md:flex-row md:items-center">
            <div>
              <h3 className="text-2xl font-bold text-white">Support</h3>
              <p className="text-gray-400">Report bugs, request docs for new community plugins, and track workstreams.</p>
            </div>
            <Link
              to="https://github.com/mleem97/FrikaModFramework/issues"
              className="inline-flex items-center gap-2 rounded border border-brandPink-900 bg-brandPink-800 px-5 py-3 font-bold text-white transition-colors hover:bg-brandPink-500">
              <FaLifeRing />
              Open GitHub Issues
            </Link>
          </div>
        </section>
      </main>

      <footer className="border-t border-brandPink-900 bg-brandDark py-8">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 text-sm text-gray-500 md:flex-row">
          <div className="text-2xl font-black text-gray-700">FRIKA MOD 🍪</div>
          <div>
            © 2026{' '}
            <a href="https://github.com/mleem97/FrikaModFramework" className="transition-colors hover:text-brandPink-500">
              FrikaModFramework
            </a>{' '}
            & FrikaModFramework Project. All Rights Reserved.
          </div>
        </div>
      </footer>
    </Layout>
  );
}
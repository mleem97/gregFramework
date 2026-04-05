import React, { useEffect } from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
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
      <main
        style={{
          backgroundColor: 'var(--color-app-bg)',
          backgroundImage:
            'radial-gradient(circle at 12% 6%, rgba(226, 58, 113, 0.2), transparent 36%), radial-gradient(circle at 88% 10%, rgba(226, 58, 113, 0.16), transparent 34%), radial-gradient(circle at 50% 100%, rgba(173, 20, 87, 0.14), transparent 45%)',
        }}>
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
                <div className="mb-2 text-lg font-bold text-white">{t.docsEndUserTitle}</div>
                <div className="text-sm text-gray-400">{t.docsEndUserDescription}</div>
              </Link>
              <Link to="/wiki-import/Mod-Developer-Debug" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">{t.docsModDevsTitle}</div>
                <div className="text-sm text-gray-400">{t.docsModDevsDescription}</div>
              </Link>
              <Link to="/wiki-import/Contirbutors/Contributors-Debug" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">{t.docsContributorsTitle}</div>
                <div className="text-sm text-gray-400">{t.docsContributorsDescription}</div>
              </Link>
              <Link to="/wiki-import/Framework-Features-Use-Cases" className="app-card rounded-lg p-5 text-left text-gray-200 transition-all hover:border-opacity-100 block">
                <div className="mb-2 text-lg font-bold text-white">{t.docsCapabilityTitle}</div>
                <div className="text-sm text-gray-400">{t.docsCapabilityDescription}</div>
              </Link>
            </div>
          </div>
        </section>

        {/* Ecosystem Coverage Section */}
        <section id="ecosystem" className="border-t px-4 py-20" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-app-bg)' }}>
          <div className="mx-auto max-w-6xl">
            <h2 className="mb-8 text-center text-3xl font-bold text-white">{t.ecosystemTitle}</h2>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              <article className="app-card p-6 rounded-xl border-t-2 flex flex-col h-full" style={{ borderTopColor: 'var(--color-accent-pink)' }}>
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaCode /> {t.ecosystemCoreTitle}
                </h3>
                <p className="text-sm text-gray-400 mb-6 grow">{t.ecosystemCoreDescription}</p>
                <Link to="/wiki-import/Framework-Features-Use-Cases" className="text-sm font-semibold transition-colors hover:text-white" style={{ color: 'var(--color-accent-pink)' }}>
                  {t.ecosystemCoreCta}
                </Link>
              </article>

              <article className="app-card p-6 rounded-xl border-t-2 flex flex-col h-full border-t-orange-500/50">
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaServer /> {t.ecosystemRustTitle}
                </h3>
                <p className="text-sm text-gray-400 mb-6 grow">{t.ecosystemRustDescription}</p>
                <Link to="/wiki-import/Lua-FFI-Start-Developing" className="text-sm font-semibold transition-colors hover:text-white" style={{ color: 'var(--color-accent-pink)' }}>
                  {t.ecosystemRustCta}
                </Link>
              </article>

              <article className="app-card p-6 rounded-xl border-t-2 flex flex-col h-full border-t-blue-500/50">
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaPeopleGroup /> {t.ecosystemMultiplayerTitle}
                </h3>
                <p className="text-sm text-gray-400 mb-6 grow">{t.ecosystemMultiplayerDescription}</p>
                <Link to="/wiki-import/Steamworks-P2P-Multiplayer-Roadmap" className="text-sm font-semibold transition-colors hover:text-white" style={{ color: 'var(--color-accent-pink)' }}>
                  {t.ecosystemMultiplayerCta}
                </Link>
              </article>
            </div>
          </div>
        </section>

        {/* Greg Story Section */}
        <section id="greg-story" className="border-t px-4 py-20" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-app-bg)' }}>
          <div className="mx-auto max-w-6xl">
            <div className="app-card rounded-2xl p-6 md:p-8 flex flex-col md:flex-row md:items-center md:justify-between gap-6">
              <div>
                <h2 className="mb-4 text-2xl font-bold text-white md:text-3xl">{t.gregTitle}</h2>
                <p className="mb-4 text-gray-300 text-sm md:text-base leading-relaxed max-w-md">
                  {t.gregText1}
                </p>
                <p className="mb-4 text-gray-300 text-sm md:text-base leading-relaxed max-w-md">
                  {t.gregText2}
                </p>
                <p className="text-lg font-bold text-white italic">{t.gregQuote}</p>
              </div>
              <div className="shrink-0">
                <div className="w-32 h-40 md:w-48 md:h-56 rounded-lg border flex items-center justify-center text-5xl md:text-7xl" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'rgba(226, 58, 113, 0.05)' }}>
                  👨‍💼
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Community Section */}
        <section id="community" className="border-t px-4 py-16" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-app-bg)' }}>
          <div className="mx-auto max-w-6xl">
            <div className="mb-6 rounded-xl p-4 border" style={{ borderColor: 'rgba(202, 165, 61, 0.4)', backgroundColor: 'rgba(202, 165, 61, 0.08)' }}>
              <div className="text-sm font-semibold uppercase tracking-wide text-amber-200">{t.comingSoon}</div>
              <div className="mt-1 text-base font-medium text-amber-100">{t.comingSoonText}</div>
            </div>

            <div className="app-card p-6 rounded-xl flex flex-col md:flex-row md:items-center md:justify-between gap-6">
              <div>
                <h3 className="text-2xl font-bold text-white">{t.communityTitle}</h3>
                <p className="mt-2 text-gray-400">{t.communityText}</p>
              </div>
              <div className="flex flex-wrap gap-3">
                <Link to="https://frikadellental.de" className="inline-flex items-center gap-2 rounded border px-4 py-2 font-bold text-white transition-colors" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-card-bg)' }}>
                  <FaArrowUpRightFromSquare /> frikadellental.de
                </Link>
                <Link to="/mods/standalone" className="inline-flex items-center gap-2 rounded border px-4 py-2 font-bold text-white transition-colors" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-card-bg)' }}>
                  <FaShop /> {t.availableModsLabel}
                </Link>
                <Link to="https://github.com/mleem97/FrikaModFramework" className="inline-flex items-center gap-2 rounded border px-4 py-2 font-bold text-white transition-colors" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-card-bg)' }}>
                  <FaGithub /> {t.repositoryLabel}
                </Link>
                <Link to="https://discord.gg/greg" className="inline-flex items-center gap-2 rounded px-4 py-2 font-bold text-white transition-colors" style={{ backgroundColor: '#5865F2' }}>
                  <FaDiscord /> {t.joinLabel}
                </Link>
              </div>
            </div>
          </div>
        </section>

        {/* Support Section */}
        <section id="support" className="border-t px-4 py-16" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-app-bg)' }}>
          <div className="mx-auto max-w-6xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h3 className="text-2xl font-bold text-white">{t.supportTitle}</h3>
              <p className="text-gray-400">{t.supportText}</p>
            </div>
            <Link
              to="https://github.com/mleem97/FrikaModFramework/issues"
              className="inline-flex items-center gap-2 rounded px-5 py-3 font-bold text-white transition-colors"
              style={{ backgroundColor: 'var(--color-accent-pink)' }}>
              <FaLifeRing /> {t.supportCta}
            </Link>
          </div>
        </section>
      </main>

      <footer className="border-t py-8" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'var(--color-card-bg)' }}>
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 text-sm text-gray-500 md:flex-row">
          <div className="font-bold flex items-center gap-2">
            <span className="bg-white/10 px-2 py-1 rounded text-white">FRIKA MOD 🍪</span>
          </div>
          <div>
            © 2026{' '}
            <a href="https://github.com/mleem97/FrikaModFramework" className="transition-colors hover:text-[var(--color-accent-pink)]">
              FrikaModFramework
            </a>{' '}
            & FrikaModFramework Project. All Rights Reserved.
          </div>
        </div>
      </footer>
    </Layout>
  );
}

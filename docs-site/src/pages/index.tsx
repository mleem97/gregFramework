import React, { useMemo } from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import { motion, type Variants, useReducedMotion } from 'framer-motion';
import { getHomepageContent } from '../i18n/homepage';
import gregImage from '../image.png';
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

type DocPathItem = {
  title: string;
  description: string;
  link: string;
};

const viewport = { once: true, margin: '-90px' };

function buildVariants(reducedMotion: boolean) {
  const section: Variants = reducedMotion
    ? { hidden: { opacity: 0 }, show: { opacity: 1 } }
    : {
        hidden: { opacity: 0, y: 26 },
        show: {
          opacity: 1,
          y: 0,
          transition: { duration: 0.65, ease: [0.16, 1, 0.3, 1] },
        },
      };

  const grid: Variants = {
    hidden: {},
    show: {
      transition: {
        staggerChildren: reducedMotion ? 0 : 0.09,
        delayChildren: reducedMotion ? 0 : 0.06,
      },
    },
  };

  const card: Variants = reducedMotion
    ? { hidden: { opacity: 0 }, show: { opacity: 1 } }
    : {
        hidden: { opacity: 0, y: 18, scale: 0.98 },
        show: {
          opacity: 1,
          y: 0,
          scale: 1,
          transition: { duration: 0.45, ease: [0.16, 1, 0.3, 1] },
        },
      };

  const textReveal: Variants = reducedMotion
    ? { hidden: { opacity: 0 }, show: { opacity: 1 } }
    : {
        hidden: { opacity: 0, y: 18 },
        show: {
          opacity: 1,
          y: 0,
          transition: { duration: 0.5, ease: [0.16, 1, 0.3, 1] },
        },
      };

  return { section, grid, card, textReveal };
}

export default function HomePage(): JSX.Element {
  const {
    i18n: { currentLocale },
  } = useDocusaurusContext();

  const t = getHomepageContent(currentLocale);
  const reducedMotion = useReducedMotion();
  const variants = useMemo(() => buildVariants(Boolean(reducedMotion)), [reducedMotion]);

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

  const docPaths: DocPathItem[] = [
    { title: t.docsEndUserTitle, description: t.docsEndUserDescription, link: '/wiki-import/End-User-Release' },
    { title: t.docsModDevsTitle, description: t.docsModDevsDescription, link: '/wiki-import/Mod-Developer-Debug' },
    { title: t.docsContributorsTitle, description: t.docsContributorsDescription, link: '/wiki-import/Contirbutors/Contributors-Debug' },
    { title: t.docsCapabilityTitle, description: t.docsCapabilityDescription, link: '/wiki-import/Framework-Features-Use-Cases' },
  ];

  return (
    <Layout
      title="Frika Mod Framework"
      description="Community docs for FrikaMF, standalone Rust stacks, multiplayer, and plugins.">
      <main className="bg-app-bg bg-hero-gradient min-h-screen text-gray-200">
        <section className="hero-motion-wrap relative flex min-h-[68vh] flex-col items-center justify-center overflow-hidden px-4 py-20 text-center">
          <div className="hero-particles" aria-hidden="true" />
          <div className="hero-orb hero-orb-pink" aria-hidden="true" />
          <div className="hero-orb hero-orb-green" aria-hidden="true" />

          <motion.h1
            className="homepage-logo-title text-reveal-glow mb-8 text-4xl font-black leading-none tracking-tight text-white md:text-6xl"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}>
            FRIKA MOD <span className="text-amber-700">🍪</span>
            <br />
            FRAMEWORK
          </motion.h1>

          <motion.h2
            className="mb-4 max-w-3xl text-2xl font-extrabold tracking-tight text-white md:text-4xl"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}
            transition={{ delay: reducedMotion ? 0 : 0.08 }}>
            {t.heroLine1}
            <br />
            <span className="text-gray-400">{t.heroLine2}</span>
          </motion.h2>

          <motion.p
            className="mb-10 max-w-lg text-base font-medium text-gray-400 md:text-lg"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}
            transition={{ delay: reducedMotion ? 0 : 0.14 }}>
            {t.heroSub1}
            <br />
            {t.heroSub2}
          </motion.p>

          <motion.div
            className="flex flex-wrap items-center justify-center gap-3"
            initial="hidden"
            whileInView="show"
            viewport={viewport}
            variants={variants.textReveal}
            transition={{ delay: reducedMotion ? 0 : 0.22 }}>
            <Link to="/mods/framework" className="btn-primary px-8 py-4 rounded-full text-lg font-bold shadow-lg shadow-accent-green/20">
              {t.ctaStart}
            </Link>
            <Link to="/mods/standalone" className="btn-outline px-8 py-4 rounded-full text-lg font-bold">
              {t.ctaMods}
            </Link>
          </motion.div>
        </section>

        <motion.section
          id="features"
          className="section-border px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <motion.div className="mx-auto grid max-w-6xl grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4" variants={variants.grid}>
            {features.map((feature) => (
              <motion.article
                key={feature.title}
                className="app-card app-card-motion app-card-glow p-5 rounded-xl text-gray-200"
                variants={variants.card}
                whileHover={
                  reducedMotion
                    ? undefined
                    : {
                        y: -6,
                        rotateX: 2,
                        rotateY: -2,
                        scale: 1.01,
                        transition: { type: 'spring', stiffness: 280, damping: 18 },
                      }
                }
                style={{ transformStyle: 'preserve-3d' }}>
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <span className="text-accent-pink">{feature.icon}</span>
                  <span>{feature.title}</span>
                </h3>
                <p className="text-sm font-medium text-gray-400">{feature.description}</p>
              </motion.article>
            ))}
          </motion.div>
        </motion.section>

        <motion.section
          id="docs"
          className="section-border px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-5xl text-center">
            <motion.h2 className="mb-10 text-3xl font-bold text-white" variants={variants.textReveal}>
              {t.docsPaths}
            </motion.h2>
            <motion.div className="grid grid-cols-1 gap-4 md:grid-cols-2" variants={variants.grid}>
              {docPaths.map((doc) => (
                <motion.div key={doc.link} variants={variants.card}>
                  <Link to={doc.link} className="app-card app-card-motion app-card-glow rounded-lg p-5 text-left text-gray-200 block group">
                    <div className="mb-2 text-lg font-bold text-white transition-colors group-hover:text-accent-pink">{doc.title}</div>
                    <div className="text-sm text-gray-400">{doc.description}</div>
                  </Link>
                </motion.div>
              ))}
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="ecosystem"
          className="section-border px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl">
            <motion.h2 className="mb-8 text-center text-3xl font-bold text-white" variants={variants.textReveal}>
              {t.ecosystemTitle}
            </motion.h2>
            <motion.div className="grid grid-cols-1 gap-4 md:grid-cols-3" variants={variants.grid}>
              <motion.article className="app-card app-card-motion app-card-glow p-6 rounded-xl border-t-2 flex flex-col h-full" style={{ borderTopColor: 'var(--color-accent-pink)' }} variants={variants.card}>
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaCode className="text-accent-pink" /> {t.ecosystemCoreTitle}
                </h3>
                <p className="text-sm text-gray-400 mb-6 grow">{t.ecosystemCoreDescription}</p>
                <Link to="/wiki-import/Framework-Features-Use-Cases" className="text-sm font-semibold text-accent-pink transition-colors hover:text-white">
                  {t.ecosystemCoreCta} &rarr;
                </Link>
              </motion.article>

              <motion.article className="app-card app-card-motion app-card-glow p-6 rounded-xl border-t-2 flex flex-col h-full border-t-orange-500/50" variants={variants.card}>
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaServer className="text-orange-500" /> {t.ecosystemRustTitle}
                </h3>
                <p className="text-sm text-gray-400 mb-6 grow">{t.ecosystemRustDescription}</p>
                <Link to="/wiki-import/Lua-FFI-Start-Developing" className="text-sm font-semibold text-accent-pink transition-colors hover:text-white">
                  {t.ecosystemRustCta} &rarr;
                </Link>
              </motion.article>

              <motion.article className="app-card app-card-motion app-card-glow p-6 rounded-xl border-t-2 flex flex-col h-full border-t-blue-500/50" variants={variants.card}>
                <h3 className="mb-2 flex items-center gap-2 text-lg font-bold text-white">
                  <FaPeopleGroup className="text-blue-500" /> {t.ecosystemMultiplayerTitle}
                </h3>
                <p className="text-sm text-gray-400 mb-6 grow">{t.ecosystemMultiplayerDescription}</p>
                <Link to="/wiki-import/Steamworks-P2P-Multiplayer-Roadmap" className="text-sm font-semibold text-accent-pink transition-colors hover:text-white">
                  {t.ecosystemMultiplayerCta} &rarr;
                </Link>
              </motion.article>
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="greg-story"
          className="section-border px-4 py-20"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl">
            <motion.div className="app-card app-card-glow rounded-2xl p-6 md:p-8 flex flex-col md:flex-row md:items-center md:justify-between gap-6" variants={variants.card}>
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
              <motion.div className="shrink-0" whileHover={reducedMotion ? undefined : { rotate: 1.2, y: -3 }}>
                <div className="w-32 h-40 md:w-48 md:h-56 overflow-hidden rounded-lg border" style={{ borderColor: 'var(--color-card-border)', backgroundColor: 'rgba(226, 58, 113, 0.05)' }}>
                  <img src={gregImage} alt="Greg" className="h-full w-full object-cover" />
                </div>
              </motion.div>
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="community"
          className="section-border px-4 py-16"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl">
            <motion.div className="mb-6 rounded-xl p-4 border" style={{ borderColor: 'rgba(202, 165, 61, 0.4)', backgroundColor: 'rgba(202, 165, 61, 0.08)' }} variants={variants.card}>
              <div className="text-sm font-semibold uppercase tracking-wide text-amber-200">{t.comingSoon}</div>
              <div className="mt-1 text-base font-medium text-amber-100">{t.comingSoonText}</div>
            </motion.div>

            <motion.div className="app-card app-card-glow p-6 rounded-xl flex flex-col md:flex-row md:items-center md:justify-between gap-6" variants={variants.card}>
              <div>
                <h3 className="text-2xl font-bold text-white">{t.communityTitle}</h3>
                <p className="mt-2 text-gray-400">{t.communityText}</p>
              </div>
              <div className="flex flex-wrap gap-3">
                <Link to="https://frikadellental.de" className="btn-social">
                  <FaArrowUpRightFromSquare /> frikadellental.de
                </Link>
                <Link to="/mods/standalone" className="btn-social">
                  <FaShop /> {t.availableModsLabel}
                </Link>
                <Link to="https://github.com/mleem97/FrikaModFramework" className="btn-social">
                  <FaGithub /> {t.repositoryLabel}
                </Link>
                <Link to="https://discord.gg/greg" className="btn-social bg-[#5865F2] border-transparent text-white hover:bg-[#4752C4]">
                  <FaDiscord /> {t.joinLabel}
                </Link>
              </div>
            </motion.div>
          </div>
        </motion.section>

        <motion.section
          id="support"
          className="section-border px-4 py-16"
          initial="hidden"
          whileInView="show"
          viewport={viewport}
          variants={variants.section}>
          <div className="mx-auto max-w-6xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h3 className="text-2xl font-bold text-white">{t.supportTitle}</h3>
              <p className="text-gray-400">{t.supportText}</p>
            </div>
            <motion.div whileHover={reducedMotion ? undefined : { y: -2, scale: 1.01 }}>
              <Link
                to="https://github.com/mleem97/FrikaModFramework/issues"
                className="inline-flex items-center gap-2 rounded px-5 py-3 font-bold text-white transition-colors shadow-lg shadow-accent-pink/20"
                style={{ backgroundColor: 'var(--color-accent-pink)' }}>
                <FaLifeRing /> {t.supportCta}
              </Link>
            </motion.div>
          </div>
        </motion.section>
      </main>

    </Layout>
  );
}

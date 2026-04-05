import { de } from './de';
import { en } from './en';
import { es } from './es';
import { fr } from './fr';
import { ja } from './ja';
import { ru } from './ru';
import type { HomepageContent, LocaleKey } from './types';

const homepageByLocale: Record<LocaleKey, HomepageContent> = {
  en,
  de,
  fr,
  es,
  ru,
  ja,
};

export type { HomepageContent, LocaleKey } from './types';

export function getHomepageContent(locale: string): HomepageContent {
  const normalizedLocale = (locale || 'en').toLowerCase() as LocaleKey;
  return homepageByLocale[normalizedLocale] ?? homepageByLocale.en;
}

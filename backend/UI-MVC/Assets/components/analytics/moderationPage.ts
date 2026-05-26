import { fetchModerate } from '../../services/analyticsService';
import { t } from '../../utils/adminI18n';

const MODERATION_FLAG_KEYS = ['sexual', 'hate', 'violence', 'dangerous', 'selfharm', 'pii'] as const;
type ModerationFlag = (typeof MODERATION_FLAG_KEYS)[number];

const MODERATION_FLAG_LABELS: Record<ModerationFlag, string> = {
  sexual: 'analytics.flagSexual',
  hate: 'analytics.flagHate',
  violence: 'analytics.flagViolence',
  dangerous: 'analytics.flagDangerous',
  selfharm: 'analytics.flagSelfHarm',
  pii: 'analytics.flagPii',
};

const FLAG_FALLBACKS: Record<ModerationFlag, string> = {
  sexual: 'Sexual Content',
  hate: 'Hate & Discrimination',
  violence: 'Violence & Threats',
  dangerous: 'Dangerous / Criminal',
  selfharm: 'Self Harm',
  pii: 'PII',
};

function flagLabel(flag: string): string {
  return t(MODERATION_FLAG_LABELS[flag as ModerationFlag] || '', FLAG_FALLBACKS[flag as ModerationFlag] || flag);
}

class ModerationPage {
  private denyModal: HTMLElement | null;
  private denyItemLabel: HTMLElement | null;
  private denyFreeText: HTMLTextAreaElement | null;
  private denyFlagCheckboxes: NodeListOf<HTMLInputElement>;
  private denyModalCancel: HTMLElement | null;
  private denyModalConfirm: HTMLElement | null;

  private pendingDenyType: string | null = null;
  private pendingDenyId: number | null = null;
  private pendingDenyCard: HTMLElement | null = null;

  constructor() {
    this.denyModal = document.getElementById('deny-reason-modal');
    this.denyItemLabel = document.getElementById('deny-modal-item-label');
    this.denyFreeText = document.getElementById('deny-free-text') as HTMLTextAreaElement | null;
    this.denyFlagCheckboxes = document.querySelectorAll('.deny-flag-checkbox');
    this.denyModalCancel = document.getElementById('deny-modal-cancel');
    this.denyModalConfirm = document.getElementById('deny-modal-confirm');

    this.bindAcceptButtons();
    this.bindDenyButtons();
    this.bindDenyModal();
  }

  private bindAcceptButtons(): void {
    document.querySelectorAll('.moderate-btn-accept').forEach(btn => {
      btn.addEventListener('click', () => {
        const type = btn.getAttribute('data-type')!;
        const id = parseInt(btn.getAttribute('data-id')!);
        const card = btn.closest('.moderation-card') as HTMLElement;
        const allBtns = card.querySelectorAll(
          'button.moderate-btn-accept, button.moderate-btn-deny'
        );

        if (!confirm(t('analytics.confirmApprove', `Approve this ${type} and make it visible to all users?`))) return;

        allBtns.forEach(b => ((b as HTMLButtonElement).disabled = true));
        this.pendingDenyCard = card;
        this.executeModeration(type, id, 'accept', null);
      });
    });
  }

  private bindDenyButtons(): void {
    document.querySelectorAll('.moderate-btn-deny').forEach(btn => {
      btn.addEventListener('click', () => {
        const card = btn.closest('.moderation-card') as HTMLElement;
        const allBtns = card.querySelectorAll(
          'button.moderate-btn-accept, button.moderate-btn-deny'
        );
        allBtns.forEach(b => ((b as HTMLButtonElement).disabled = true));
        this.openDenyModal(card);
      });
    });
  }

  private bindDenyModal(): void {
    this.denyModalCancel?.addEventListener('click', () => {
      this.closeDenyModal();
      this.reenableCardButtons();
    });

    this.denyModal?.addEventListener('click', e => {
      if (e.target === this.denyModal) {
        this.closeDenyModal();
        this.reenableCardButtons();
      }
    });

    this.denyModalConfirm?.addEventListener('click', () => {
      const reason = this.collectReason();
      this.closeDenyModal();
      this.executeModeration(this.pendingDenyType!, this.pendingDenyId!, 'deny', reason);
    });
  }

  private collectReason(): string | null {
    const parts: string[] = [];
    this.denyFlagCheckboxes.forEach(cb => {
      if (cb.checked) {
        const label = flagLabel(cb.dataset.flag!);
        parts.push(label);
      }
    });
    const freeText = this.denyFreeText?.value.trim();
    if (freeText) parts.push(freeText);
    return parts.length > 0 ? parts.join('; ') : null;
  }

  private resetDenyModal(): void {
    this.denyFlagCheckboxes.forEach(cb => {
      cb.checked = false;
    });
    if (this.denyFreeText) this.denyFreeText.value = '';
  }

  private openDenyModal(card: HTMLElement): void {
    this.resetDenyModal();

    const preSelectFlags = [
      card.dataset.flagSexual === 'true' ? 'sexual' : null,
      card.dataset.flagHate === 'true' ? 'hate' : null,
      card.dataset.flagViolence === 'true' ? 'violence' : null,
      card.dataset.flagDangerous === 'true' ? 'dangerous' : null,
      card.dataset.flagSelfharm === 'true' ? 'selfharm' : null,
      card.dataset.flagPii === 'true' ? 'pii' : null,
    ].filter(Boolean) as string[];

    if (preSelectFlags.length > 0) {
      this.denyFlagCheckboxes.forEach(cb => {
        if (preSelectFlags.includes(cb.dataset.flag!)) cb.checked = true;
      });
    }

    const prevReason = card.dataset.rejectionReason;
    if (prevReason && prevReason !== '' && this.denyFreeText) {
      this.denyFreeText.value = prevReason;
    }

    this.pendingDenyType = card.dataset.type!;
    this.pendingDenyId = parseInt(card.dataset.id!);
    this.pendingDenyCard = card;

    const typeLabel =
      (card.dataset.type === 'idea' ? t('analytics.idea', 'Idea') : t('analytics.comment', 'Comment')) +
      ' #' +
      card.dataset.id;
    if (this.denyItemLabel) this.denyItemLabel.textContent = typeLabel;

    this.denyModal?.classList.remove('hidden');
    this.denyModal?.classList.add('flex');
  }

  private closeDenyModal(): void {
    this.denyModal?.classList.add('hidden');
    this.denyModal?.classList.remove('flex');
  }

  private reenableCardButtons(): void {
    if (!this.pendingDenyCard) return;
    const allBtns = this.pendingDenyCard.querySelectorAll(
      'button.moderate-btn-accept, button.moderate-btn-deny'
    );
    allBtns.forEach(b => ((b as HTMLButtonElement).disabled = false));
  }

  private async executeModeration(
    type: string,
    id: number,
    action: string,
    reason: string | null
  ): Promise<void> {
    try {
      const ok = await fetchModerate(type, id, action, reason);
      if (ok) {
        this.collapseCard(this.pendingDenyCard!, action);
      } else {
        this.showCardError(t('analytics.moderationError', 'Error'));
        this.reenableCardButtons();
      }
    } catch {
      this.showCardError(t('analytics.networkError', 'Network error'));
      this.reenableCardButtons();
    }
  }

  private showCardError(message: string): void {
    if (!this.pendingDenyCard) return;
    const resultEl = this.pendingDenyCard.querySelector('.moderation-result') as HTMLElement | null;
    if (resultEl) {
      resultEl.textContent = message;
      resultEl.classList.remove('hidden');
      resultEl.className = 'moderation-result text-sm font-semibold ml-2 text-red-600';
    }
  }

  private collapseCard(card: HTMLElement, action: string): void {
    const resultEl = card.querySelector('.moderation-result') as HTMLElement | null;
    if (resultEl) {
      resultEl.textContent =
        action === 'accept'
          ? t('analytics.approved', 'Approved')
          : t('analytics.rejected', 'Rejected');
      resultEl.classList.remove('hidden');
      resultEl.className =
        'moderation-result text-sm font-semibold ml-2 ' +
        (action === 'accept' ? 'text-green-600' : 'text-red-600');
    }

    card.style.opacity = '0.5';
    card.style.transition = 'opacity 0.3s ease';

    setTimeout(() => {
      card.style.maxHeight = card.offsetHeight + 'px';
      card.style.overflow = 'hidden';
      card.style.transition =
        'max-height 0.3s ease, opacity 0.3s ease, margin 0.3s ease, padding 0.3s ease';

      requestAnimationFrame(() => {
        card.style.maxHeight = '0';
        card.style.opacity = '0';
        card.style.margin = '0';
        card.style.padding = '0';
      });

      setTimeout(() => {
        card.remove();
        this.updateCountAndEmptyState();
      }, 350);
    }, 400);
  }

  private updateCountAndEmptyState(): void {
    const remaining = document.querySelectorAll('.moderation-card').length;
    const countEl = document.querySelector('main p.text-secondary');
    if (countEl) {
      countEl.textContent =
        remaining +
        ' ' +
        t('analytics.itemPending', 'item') +
        (remaining !== 1 ? 's' : '') +
        ' ' +
        t('analytics.pendingReview', 'pending review');
    }

    if (remaining === 0) {
      const main = document.querySelector('main');
      const spaceDiv = main?.querySelector('.space-y-4');
      if (spaceDiv) {
        spaceDiv.outerHTML =
          '<div class="bg-card rounded-xl border border-secondary/10 p-12 text-center">' +
          '<div class="w-16 h-16 mx-auto mb-4 rounded-full bg-secondary/5 flex items-center justify-center">' +
          '<svg class="w-8 h-8 text-secondary/30" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' +
          '</div>' +
          `<h3 class="text-lg font-semibold text-text mb-1">${t('analytics.allClear', 'All clear')}</h3>` +
          `<p class="text-sm text-secondary">${t('analytics.noPendingItems', 'No ideas or comments are currently pending review.')}</p>` +
          '</div>';
      }
    }
  }
}

document.addEventListener('DOMContentLoaded', () => {
  if (document.getElementById('deny-reason-modal')) {
    new ModerationPage();
  }
});

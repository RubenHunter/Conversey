interface PlatformWorkspaceStat {
  workspaceSlug: string;
  workspaceName: string;
  projectCount: number;
  youthCount: number;
  ideaCount: number;
  answerCount: number;
  conversionRate: number;
}

const palette = [
  '#6366f1', '#8b5cf6', '#d946ef', '#ec4899', '#f43f5e',
  '#f97316', '#eab308', '#22c55e', '#14b8a6', '#06b6d4',
  '#3b82f6', '#6366f1', '#a855f7', '#f472b6', '#fb923c'
];

function getColor(index: number): string {
  return palette[index % palette.length];
}

function createPlatformComparisonChart(data: PlatformWorkspaceStat[]): void {
  const canvas = document.getElementById('platform-comparison-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  new (window as any).Chart(canvas, {
    type: 'bar',
    data: {
      labels: data.map(d => d.workspaceName),
      datasets: [
        {
          label: 'Youth',
          data: data.map(d => d.youthCount),
          backgroundColor: getColor(0),
          borderRadius: 4
        },
        {
          label: 'Ideas',
          data: data.map(d => d.ideaCount),
          backgroundColor: getColor(1),
          borderRadius: 4
        },
        {
          label: 'Answers',
          data: data.map(d => d.answerCount),
          backgroundColor: getColor(2),
          borderRadius: 4
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: { beginAtZero: true, ticks: { stepSize: 1 } }
      },
      plugins: {
        legend: { position: 'bottom' }
      }
    }
  });
}

document.addEventListener('DOMContentLoaded', () => {
  const statsEl = document.getElementById('platform-stats-data');
  if (!statsEl?.textContent) return;

  try {
    const data: PlatformWorkspaceStat[] = JSON.parse(statsEl.textContent);
    if (data.length > 0) {
      createPlatformComparisonChart(data);
    }
  } catch (e) {
    console.error('Failed to parse platform stats', e);
  }
});

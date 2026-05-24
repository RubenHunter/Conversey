import type { QuestionComponent } from './singleChoiceQuestion'
import { generateQuestionHeader, initQuestionSpeakerForWrapper } from '../utils/surveyUtils'
import {RangeQuestion} from "../../../models/Question.ts";

export function renderScaleQuestion(question: RangeQuestion, index: number): QuestionComponent {
    let scaleValue: number | null = null
    let answerCallback: (() => void) | null = null
    let isLocked = false

    const lower: number = question.min ?? 1
    const upper: number = question.max ?? 10
    const totalSteps = upper - lower + 1

    const wrapper = document.createElement('div')
    wrapper.setAttribute('data-question-index', String(index))
    wrapper.className = 'survey-question-group scale-question'

    wrapper.innerHTML = `
        ${generateQuestionHeader(question, index + 1)}
        <div class="scale-row">
            <div class="scale-slider-area" id="scale-area-${question.id}">
                <div class="scale-track" id="scale-track-${question.id}">
                    <div class="scale-dots" id="scale-dots-${question.id}"></div>
                    <div class="scale-selector" id="scale-selector-${question.id}"></div>
                    <div class="scale-bubble" id="scale-bubble-${question.id}"></div>
                    <input
                        id="scale-${question.id}"
                        class="scale-input"
                        type="range"
                        min="${lower}"
                        max="${upper}"
                        step="1"
                        value="${Math.round((lower + upper) / 2)}"
                    />
                </div>
                <div class="scale-ticks" id="scale-ticks-${question.id}"></div>
            </div>
            <input
                id="scale-num-${question.id}"
                class="scale-number-input"
                type="number"
                min="${lower}"
                max="${upper}"
                placeholder="${lower}–${upper}"
            />
        </div>
        <p class="survey-error" id="error-${question.id}">
            Please select a value to continue.
        </p>
    `

    initQuestionSpeakerForWrapper(wrapper)

    const rangeInput = wrapper.querySelector<HTMLInputElement>(`#scale-${question.id}`)!
    const numInput = wrapper.querySelector<HTMLInputElement>(`#scale-num-${question.id}`)!
    const track = wrapper.querySelector<HTMLDivElement>(`#scale-track-${question.id}`)!
    const dotsEl = wrapper.querySelector<HTMLDivElement>(`#scale-dots-${question.id}`)!
    const selectorEl = wrapper.querySelector<HTMLDivElement>(`#scale-selector-${question.id}`)!
    const bubbleEl = wrapper.querySelector<HTMLDivElement>(`#scale-bubble-${question.id}`)!
    const ticksEl = wrapper.querySelector<HTMLDivElement>(`#scale-ticks-${question.id}`)!

    // Build dots
    const SPARSE_THRESHOLD = 30
    if (totalSteps <= SPARSE_THRESHOLD) {
        for (let i = lower; i <= upper; i++) {
            const d = document.createElement('div')
            d.className = 'scale-dot'
            d.dataset.val = String(i)
            dotsEl.appendChild(d)
        }
    } else {
        // Sparse dots at round intervals
        dotsEl.style.position = 'absolute'
        dotsEl.style.padding = '0'
        dotsEl.style.left = '16px'
        dotsEl.style.right = '16px'
        dotsEl.style.display = 'block'
        const interval = totalSteps <= 50 ? 5 : 10
        for (let i = lower; i <= upper; i++) {
            if (i === lower || i === upper || i % interval === 0) {
                const d = document.createElement('div')
                d.className = 'scale-dot'
                d.dataset.val = String(i)
                d.style.position = 'absolute'
                d.style.top = '50%'
                d.style.transform = 'translate(-50%, -50%)'
                d.style.left = `${pct(i) * 100}%`
                dotsEl.appendChild(d)
            }
        }
    }

    function pct(val: number): number {
        return (val - lower) / (upper - lower)
    }

    function buildTicks(): void {
        ticksEl.innerHTML = ''
        const avail = track.offsetWidth - 32
        const minPxPerLabel = 28
        const maxLabels = Math.floor(avail / minPxPerLabel)

        let stops: number[] = []
        if (totalSteps <= maxLabels) {
            for (let i = lower; i <= upper; i++) stops.push(i)
        } else if (maxLabels >= 3) {
            const step = Math.ceil(totalSteps / (maxLabels - 1))
            for (let i = lower; i <= upper; i += step) stops.push(i)
            if (stops[stops.length - 1] !== upper) stops.push(upper)
        } else {
            ticksEl.style.display = 'none'
            return
        }

        ticksEl.style.display = 'block'
        ticksEl.style.position = 'relative'
        ticksEl.style.height = '16px'

        stops.forEach(v => {
            const t = document.createElement('span')
            t.className = 'scale-tick-label'
            t.textContent = String(v)
            t.style.position = 'absolute'
            t.style.left = `${16 + pct(v) * (track.offsetWidth - 32)}px`
            t.style.transform = 'translateX(-50%)'
            ticksEl.appendChild(t)
        })
    }

    // Smooth bubble animation
    let bubbleX = 0.5
    let targetX = 0.5
    let rafId: number | null = null
    let isSliding = false

    function animateBubble(): void {
        const diff = targetX - bubbleX
        if (Math.abs(diff) < 0.0005) {
            bubbleX = targetX
            rafId = null
            if (!isSliding) bubbleEl.classList.remove('scale-bubble--show')
            return
        }
        bubbleX += diff * 0.25
        applyBubblePos()
        rafId = requestAnimationFrame(animateBubble)
    }

    function applyBubblePos(): void {
        const tw = track.offsetWidth - 32
        bubbleEl.style.left = `${16 + bubbleX * tw}px`
    }

    function updateUI(val: number, sliding: boolean): void {
        isSliding = sliding
        const p = pct(val)
        targetX = p

        const tw = track.offsetWidth - 32
        selectorEl.style.left = `${16 + p * tw}px`
        selectorEl.style.opacity = '1'

        bubbleEl.textContent = String(val)
        if (sliding) {
            bubbleEl.classList.add('scale-bubble--show')
            if (!rafId) rafId = requestAnimationFrame(animateBubble)
        }

        dotsEl.querySelectorAll<HTMLElement>('.scale-dot').forEach(d => {
            const dv = parseInt(d.dataset.val ?? '0')
            d.classList.toggle('scale-dot--active', dv <= val)
        })
    }

    function hideBubble(): void {
        isSliding = false
        if (!rafId) bubbleEl.classList.remove('scale-bubble--show')
    }

    rangeInput.addEventListener('input', (e) => {
        if (isLocked) {
            e.preventDefault()
            return
        }
        const val = parseInt(rangeInput.value)
        scaleValue = val
        numInput.value = String(val)
        updateUI(val, true)
        wrapper.querySelector(`#error-${question.id}`)?.classList.remove('show')
        answerCallback?.()
    })

    rangeInput.addEventListener('mouseup', hideBubble)
    rangeInput.addEventListener('touchend', hideBubble)

    numInput.addEventListener('input', (e) => {
        if (isLocked) {
            e.preventDefault()
            return
        }
        const val = parseInt(numInput.value)
        if (isNaN(val)) return
        const clamped = Math.max(lower, Math.min(upper, val))
        scaleValue = clamped
        rangeInput.value = String(clamped)
        updateUI(clamped, false)
        wrapper.querySelector(`#error-${question.id}`)?.classList.remove('show')
        answerCallback?.()
    })

    numInput.addEventListener('blur', (e) => {
        if (isLocked) {
            e.preventDefault()
            return
        }
        let val = parseInt(numInput.value)
        if (isNaN(val)) { numInput.value = ''; return }
        val = Math.max(lower, Math.min(upper, val))
        numInput.value = String(val)
        scaleValue = val
        rangeInput.value = String(val)
        updateUI(val, false)
    })

    window.addEventListener('resize', () => {
        buildTicks()
        if (scaleValue !== null) updateUI(scaleValue, false)
        applyBubblePos()
    })

    // Init after layout
    requestAnimationFrame(() => {
        buildTicks()
        applyBubblePos()
        selectorEl.style.opacity = '0'
    })

    function applyScaleValue(val: number | null): void {
        scaleValue = val
        if (val !== null) {
            rangeInput.value = String(val)
            numInput.value = String(val)
            updateUI(val, false)
        } else {
            numInput.value = ''
            selectorEl.style.opacity = '0'
        }
        if (val !== null) {
            wrapper.querySelector(`#error-${question.id}`)?.classList.remove('show')
        }
    }

    return {
        getAnswer: () => scaleValue,
        validate: () => {
            if (question.required && scaleValue === null) {
                wrapper.querySelector(`#error-${question.id}`)?.classList.add('show')
                return false
            }
            return true
        },
        lock: () => { 
            isLocked = true
            rangeInput.disabled = true
            numInput.disabled = true
            wrapper.classList.add('scale-question--locked')
            wrapper.style.opacity = '0.4'
            wrapper.style.pointerEvents = 'none'
        },
        unlock: () => { 
            isLocked = false
            rangeInput.disabled = false
            numInput.disabled = false
            wrapper.classList.remove('scale-question--locked')
            wrapper.style.opacity = '1'
            wrapper.style.pointerEvents = 'auto'
        },
        onAnswer: (cb) => { answerCallback = cb },
        setAnswer: (answer) => {
            applyScaleValue(typeof answer === 'number' && Number.isFinite(answer) ? answer : null)
        },
        getElement: () => wrapper,
    }
}
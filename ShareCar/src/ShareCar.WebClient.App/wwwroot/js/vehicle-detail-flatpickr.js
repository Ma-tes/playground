// Flatpickr integration for start/end reservation times with blocked datetime ranges.

function initReservationPicker() {
    const startInput = document.getElementById('startTime');
    const endInput = document.getElementById('endTime');

    if (!startInput || !endInput || typeof flatpickr !== 'function') {
        return;
    }

    const blockedRanges = parseJsonArray(startInput.dataset.blockedRanges)
        .map(normalizeRange)
        .filter((range) => range);

    const blockedDaySet = new Set();
    const blockedTimeMap = new Map();

    blockedRanges.forEach((range) => {
        const days = splitRangeByDay(range);
        days.forEach((segment) => {
            const ymd = formatYmd(segment.start);
            blockedTimeMap.set(ymd, (blockedTimeMap.get(ymd) || []).concat([segment]));
            if (isFullDay(segment.start, segment.end)) {
                blockedDaySet.add(ymd);
            }
        });
    });

    blockedTimeMap.forEach((ranges, ymd) => {
        const merged = mergeDayRanges(ranges);
        blockedTimeMap.set(ymd, merged);
        if (merged.length === 1 && isFullDayRange(merged[0])) {
            blockedDaySet.add(ymd);
        }
    });

    function parseJsonArray(value) {
        if (!value || value.trim() === '') {
            return [];
        }
        try {
            const parsed = JSON.parse(value);
            return Array.isArray(parsed) ? parsed : [];
        } catch (error) {
            return [];
        }
    }

    function normalizeRange(range) {
        if (!range || typeof range.from !== 'string' || typeof range.to !== 'string') {
            return null;
        }
        const start = parseDateTime(range.from);
        const end = parseDateTime(range.to);
        if (!start || !end || end <= start) {
            return null;
        }
        return { start, end };
    }

    function parseDateTime(value) {
        if (!value || typeof value !== 'string') {
            return null;
        }
        const normalized = value.replace('T', ' ').trim();
        const parts = normalized.split(' ');
        if (parts.length < 2) {
            return null;
        }
        const [datePart, timePart] = parts;
        const [year, month, day] = datePart.split('-').map((part) => parseInt(part, 10));
        const [hour, minute] = timePart.split(':').map((part) => parseInt(part, 10));
        if ([year, month, day, hour, minute].some((num) => Number.isNaN(num))) {
            return null;
        }
        return new Date(year, month - 1, day, hour, minute);
    }

    function formatYmd(date) {
        const y = date.getFullYear();
        const m = `${date.getMonth() + 1}`.padStart(2, '0');
        const d = `${date.getDate()}`.padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    function isFullDay(start, end) {
        return start.getHours() === 0 && start.getMinutes() === 0 && end.getHours() === 23 && end.getMinutes() === 59;
    }

    function isFullDayRange(range) {
        return isFullDay(range.start, range.end);
    }

    function splitRangeByDay(range) {
        const segments = [];
        let cursor = new Date(range.start.getTime());
        while (formatYmd(cursor) !== formatYmd(range.end)) {
            const endOfDay = new Date(cursor.getFullYear(), cursor.getMonth(), cursor.getDate(), 23, 59);
            segments.push({ start: new Date(cursor.getTime()), end: endOfDay });
            cursor = new Date(cursor.getFullYear(), cursor.getMonth(), cursor.getDate() + 1, 0, 0);
        }
        segments.push({ start: new Date(cursor.getTime()), end: new Date(range.end.getTime()) });
        return segments;
    }

    function mergeDayRanges(ranges) {
        const sorted = ranges
            .map((range) => ({ start: range.start, end: range.end }))
            .sort((a, b) => a.start - b.start);
        const merged = [];
        sorted.forEach((range) => {
            if (!merged.length) {
                merged.push(range);
                return;
            }
            const last = merged[merged.length - 1];
            if (range.start <= last.end) {
                last.end = new Date(Math.max(last.end.getTime(), range.end.getTime()));
            } else {
                merged.push(range);
            }
        });
        return merged;
    }

    function isFullDayBlocked(date) {
        return blockedDaySet.has(formatYmd(date));
    }

    function isTimeBlocked(date) {
        const ymd = formatYmd(date);
        const ranges = blockedTimeMap.get(ymd);
        if (!ranges) {
            return false;
        }
        return ranges.some((range) => date >= range.start && date < range.end);
    }

    function onDayCreate(_, __, ___, dayElem) {
        const date = dayElem.dateObj;
        const ymd = formatYmd(date);
        if (blockedDaySet.has(ymd)) {
            dayElem.classList.add('flatpickr-blocked-day');
            dayElem.title = 'Blocked all day';
        } else if (blockedTimeMap.has(ymd)) {
            dayElem.classList.add('flatpickr-partial-blocked-day');
            dayElem.title = 'Partially blocked';
        }
    }

    function renderBlockedTimes(instance, date) {
        const container = instance.calendarContainer;
        if (!container) {
            return;
        }
        let list = container.querySelector('.flatpickr-blocked-times');
        if (!list) {
            list = document.createElement('div');
            list.className = 'flatpickr-blocked-times';
            container.appendChild(list);
        }

        if (!date) {
            list.classList.add('is-hidden');
            list.textContent = '';
            return;
        }

        const ymd = formatYmd(date);
        const ranges = blockedTimeMap.get(ymd);
        if (!ranges || ranges.length === 0 || blockedDaySet.has(ymd)) {
            list.classList.add('is-hidden');
            list.textContent = '';
            return;
        }

        const text = ranges.map((range) => `${labelTime(range.start)}-${labelTime(range.end)}`).join(', ');
        list.classList.remove('is-hidden');
        list.innerHTML = `Blocked hours: <span class="flatpickr-blocked-time">${text}</span>`;
    }

    function labelTime(date) {
        const hours = date.getHours();
        const mins = date.getMinutes();
        return `${`${hours}`.padStart(2, '0')}:${`${mins}`.padStart(2, '0')}`;
    }

    function findNextAvailableTime(date, instance) {
        const minuteIncrement = instance?.config?.minuteIncrement || 5;
        const maxIterations = Math.ceil((60 / minuteIncrement) * 24 * 31);
        const candidate = new Date(date.getTime());
        candidate.setSeconds(0, 0);

        for (let i = 0; i < maxIterations; i += 1) {
            if (!isFullDayBlocked(candidate) && !isTimeBlocked(candidate)) {
                return candidate;
            }
            if (isFullDayBlocked(candidate)) {
                candidate.setDate(candidate.getDate() + 1);
                candidate.setHours(0, 0, 0, 0);
                continue;
            }
            candidate.setMinutes(candidate.getMinutes() + minuteIncrement);
        }

        return null;
    }

    function adjustToAvailable(date, instance) {
        if (!isFullDayBlocked(date) && !isTimeBlocked(date)) {
            return;
        }

        const nextAvailable = findNextAvailableTime(date, instance);
        if (!nextAvailable) {
            instance.clear();
            return;
        }

        instance._adjusting = true;
        instance.setDate(nextAvailable, true);
        instance._adjusting = false;
    }

    function applyPicker(input) {
        return flatpickr(input, {
            enableTime: true,
            dateFormat: 'Y-m-d H:i',
            minDate: 'today',
            time_24hr: true,
            disable: [isFullDayBlocked],
            onDayCreate,
            onOpen(selectedDates, _, instance) {
                renderBlockedTimes(instance, selectedDates[0]);
            },
            onChange(selectedDates, _, instance) {
                if (instance._adjusting) {
                    return;
                }
                renderBlockedTimes(instance, selectedDates[0]);
                if (!selectedDates.length) {
                    return;
                }
                const date = selectedDates[0];
                adjustToAvailable(date, instance);
            }
        });
    }

    applyPicker(startInput);
    applyPicker(endInput);
}

window.initReservationPicker = initReservationPicker;

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initReservationPicker);
} else {
    initReservationPicker();
}

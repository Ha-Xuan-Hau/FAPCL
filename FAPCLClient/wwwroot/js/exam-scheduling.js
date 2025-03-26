document.addEventListener('DOMContentLoaded', function () {
    // Elements
    const examTypeSelect = document.getElementById('examTypeSelect');
    const examNameField = document.getElementById('examNameField');
    const courseSelectionContainer = document.getElementById('courseSelectionContainer');
    const addCourseBtn = document.getElementById('addCourseBtn');
    const startDatePicker = document.getElementById('startDatePicker');
    const endDatePicker = document.getElementById('endDatePicker');
    const dateRangeInfo = document.getElementById('dateRangeInfo');
    const dateRangeError = document.getElementById('dateRangeError');
    const dayCount = document.getElementById('dayCount');
    const scheduleButton = document.getElementById('scheduleButton');

    // Set exam name to match the exam type selection
    if (examTypeSelect) {
        examTypeSelect.addEventListener('change', function () {
            if (examNameField) {
                examNameField.value = this.value; // Just use the exact value from the dropdown (PE, FE, 2FE)
            }
            validateForm();
        });
    }

    // Course selection handling
    if (addCourseBtn) {
        addCourseBtn.addEventListener('click', function () {
            const courseRows = document.querySelectorAll('.course-row');
            if (courseRows.length >= 4) {
                return; // Maximum 4 courses allowed
            }

            const newRow = courseRows[0].cloneNode(true);
            const select = newRow.querySelector('select');
            const index = courseRows.length;

            // Update the name attribute with the correct index
            select.name = `SelectedCourseIds[${index}]`;
            select.value = '0';

            // Setup remove button
            const removeButton = newRow.querySelector('.remove-course');
            removeButton.style.display = 'block';
            removeButton.addEventListener('click', function () {
                courseSelectionContainer.removeChild(newRow);
                updateCourseRowIndices();
                updateRemoveButtonVisibility();
                validateForm();
            });

            courseSelectionContainer.appendChild(newRow);
            updateRemoveButtonVisibility();
            validateForm();
        });
    }

    // Setup existing remove buttons
    document.querySelectorAll('.remove-course').forEach(button => {
        button.addEventListener('click', function () {
            const row = this.closest('.course-row');
            if (row && courseSelectionContainer) {
                courseSelectionContainer.removeChild(row);
                updateCourseRowIndices();
                updateRemoveButtonVisibility();
                validateForm();
            }
        });
    });

    // Update course indices after removal
    function updateCourseRowIndices() {
        const courseRows = document.querySelectorAll('.course-row');
        courseRows.forEach((row, index) => {
            const select = row.querySelector('select');
            if (select) {
                select.name = `SelectedCourseIds[${index}]`;
            }
        });
    }

    // Show/hide remove buttons based on course count
    function updateRemoveButtonVisibility() {
        const courseRows = document.querySelectorAll('.course-row');
        if (courseRows.length === 1) {
            const removeBtn = courseRows[0].querySelector('.remove-course');
            if (removeBtn) removeBtn.style.display = 'none';
        } else {
            courseRows.forEach(row => {
                const removeBtn = row.querySelector('.remove-course');
                if (removeBtn) removeBtn.style.display = 'block';
            });
        }

        // Show/hide add button
        if (addCourseBtn) {
            addCourseBtn.style.display = courseRows.length >= 4 ? 'none' : 'block';
        }
    }

    // Date range validation
    function validateDateRange() {
        if (!startDatePicker || !endDatePicker || !startDatePicker.value || !endDatePicker.value) {
            if (dateRangeInfo) dateRangeInfo.style.display = 'none';
            if (dateRangeError) dateRangeError.style.display = 'none';
            return false;
        }

        const start = new Date(startDatePicker.value);
        const end = new Date(endDatePicker.value);

        if (start > end) {
            if (dateRangeInfo) dateRangeInfo.style.display = 'none';
            if (dateRangeError) {
                dateRangeError.textContent = 'End date must be after start date';
                dateRangeError.style.display = 'block';
            }
            return false;
        }

        // Calculate days difference (inclusive)
        const diffTime = Math.abs(end - start);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;

        if (dayCount) dayCount.textContent = diffDays;

        if (diffDays > 14) {
            if (dateRangeInfo) dateRangeInfo.style.display = 'none';
            if (dateRangeError) {
                dateRangeError.textContent = 'Exam period cannot exceed 14 days';
                dateRangeError.style.display = 'block';
            }
            return false;
        } else {
            if (dateRangeInfo) dateRangeInfo.style.display = 'block';
            if (dateRangeError) dateRangeError.style.display = 'none';
            return true;
        }
    }

    if (startDatePicker) {
        startDatePicker.addEventListener('change', function () {
            validateDateRange();
            validateForm();
        });
    }

    if (endDatePicker) {
        endDatePicker.addEventListener('change', function () {
            validateDateRange();
            validateForm();
        });
    }

    // Course selection validation
    function hasSelectedCourses() {
        const selects = document.querySelectorAll('.course-select');
        for (let select of selects) {
            if (select.value && select.value !== '0') {
                return true;
            }
        }
        return false;
    }

    // Validate entire form
    function validateForm() {
        if (!scheduleButton) return;

        const isExamTypeSelected = examTypeSelect && examTypeSelect.value !== '';
        const isDateRangeValid = validateDateRange();
        const hasCourses = hasSelectedCourses();

        scheduleButton.disabled = !(isExamTypeSelected && isDateRangeValid && hasCourses);
    }

    // Initialize on page load
    updateRemoveButtonVisibility();
    validateForm();

    // Add event listeners to all course selects
    document.querySelectorAll('.course-select').forEach(select => {
        select.addEventListener('change', validateForm);
    });
});

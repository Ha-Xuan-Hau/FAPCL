document.addEventListener("DOMContentLoaded", function () {
    // Elements
    const examTypeSelect = document.getElementById("examTypeSelect");
    const examNameField = document.getElementById("examNameField");
    const courseSelectionContainer = document.getElementById("courseSelectionContainer");
    const addCourseBtn = document.getElementById("addCourseBtn");
    const scheduleButton = document.getElementById("scheduleButton");

    // Set exam name to match exam type selection (if needed)
    if (examTypeSelect) {
        examTypeSelect.addEventListener("change", function () {
            if (examNameField) {
                examNameField.value = this.value;
            }
            validateForm();
        });
    }

    // Add event listeners to existing course selects
    document.querySelectorAll(".course-select").forEach((select) => {
        select.addEventListener("change", function () {
            updateAvailableCourses();
            validateForm();
        });
    });

    // Add Course button handler
    if (addCourseBtn) {
        addCourseBtn.addEventListener("click", function () {
            const courseRows = document.querySelectorAll(".course-row");
            if (courseRows.length >= 4) return; // maximum of 4 courses allowed

            // Clone the first course row
            const newRow = courseRows[0].cloneNode(true);
            const select = newRow.querySelector("select");
            const index = courseRows.length;

            // Update the name attribute for proper model binding
            select.name = `SelectedCourseIds[${index}]`;
            select.value = "0"; // reset selection

            // Attach event listener to the new select
            select.addEventListener("change", function () {
                updateAvailableCourses();
                validateForm();
            });

            // Setup remove button for the new row
            const removeButton = newRow.querySelector(".remove-course");
            removeButton.style.display = "block";
            removeButton.addEventListener("click", function () {
                courseSelectionContainer.removeChild(newRow);
                updateCourseRowIndices();
                updateRemoveButtonVisibility();
                updateAvailableCourses();
                validateForm();
            });

            courseSelectionContainer.appendChild(newRow);
            updateRemoveButtonVisibility();
            updateAvailableCourses();
            validateForm();
        });
    }

    // Remove button initial setup
    document.querySelectorAll(".remove-course").forEach(button => {
        button.addEventListener("click", function () {
            const row = this.closest(".course-row");
            if (row && courseSelectionContainer) {
                courseSelectionContainer.removeChild(row);
                updateCourseRowIndices();
                updateRemoveButtonVisibility();
                updateAvailableCourses();
                validateForm();
            }
        });
    });

    // Update indices for course rows after removal
    function updateCourseRowIndices() {
        const courseRows = document.querySelectorAll(".course-row");
        courseRows.forEach((row, index) => {
            const select = row.querySelector("select");
            if (select) {
                select.name = `SelectedCourseIds[${index}]`;
            }
        });
    }

    // Show/hide remove buttons and add button based on course count
    function updateRemoveButtonVisibility() {
        const courseRows = document.querySelectorAll(".course-row");
        if (courseRows.length === 1) {
            const removeBtn = courseRows[0].querySelector(".remove-course");
            if (removeBtn) removeBtn.style.display = "none";
        } else {
            courseRows.forEach(row => {
                const removeBtn = row.querySelector(".remove-course");
                if (removeBtn) removeBtn.style.display = "block";
            });
        }
        // Hide add button if maximum reached
        if (addCourseBtn) {
            addCourseBtn.style.display = courseRows.length >= 4 ? "none" : "block";
        }
    }

    // Function to update each course-select so that already selected options are disabled
    function updateAvailableCourses() {
        const selects = document.querySelectorAll(".course-select");
        const selectedValues = [];

        // Gather currently selected course values (ignore the default '0')
        selects.forEach(select => {
            if (select.value !== "0") {
                selectedValues.push(select.value);
            }
        });

        // For each dropdown, disable options that have been selected in another dropdown
        selects.forEach(select => {
            const currentValue = select.value;
            Array.from(select.options).forEach(option => {
                const optionValue = option.value;
                // Always enable the default and the currently selected option
                if (optionValue === "0" || optionValue === currentValue) {
                    option.disabled = false;
                    return;
                }
                const isSelectedElsewhere = selectedValues.includes(optionValue) && optionValue !== currentValue;
                option.disabled = isSelectedElsewhere;
                if (isSelectedElsewhere) {
                    if (!option.dataset.originalText) {
                        option.dataset.originalText = option.textContent;
                    }
                    option.textContent = `${option.dataset.originalText} (already selected)`;
                } else if (option.dataset.originalText) {
                    option.textContent = option.dataset.originalText;
                }
            });
        });
    }

    // (Optional) Validate the overall form – enable or disable the schedule button.
    function validateForm() {
        if (!scheduleButton) return;
        const isExamTypeSelected = examTypeSelect && examTypeSelect.value !== "";
        const hasCourses = Array.from(document.querySelectorAll(".course-select"))
                             .some(select => select.value !== "0");
        scheduleButton.disabled = !(isExamTypeSelected && hasCourses);
    }

    // Initial calls
    updateRemoveButtonVisibility();
    updateAvailableCourses();
    validateForm();
});

$(function () {
    var dt_criminal_table = $('.datatables-users');
    var statsContainer = $('#criminal-stats'); // Make sure you have a div with this ID where cards will be inserted.

    // Fetch and display criminal statistics
    function fetchCriminalStats() {
        $.ajax({
            url: '/api/CriminalAPI',
            method: 'GET',
            success: function (response) {
                if (response.data) {
                    let criminals = response.data;
                    let todayCount = 0, lastWeekCount = 0, lastMonthCount = 0;
                    let yesterdayCount = 0, prevWeekCount = 0, prevMonthCount = 0;

                    let today = new Date();
                    let yesterday = new Date();
                    yesterday.setDate(today.getDate() - 1);

                    let lastWeek = new Date();
                    lastWeek.setDate(today.getDate() - 7);
                    let prevWeekStart = new Date();
                    prevWeekStart.setDate(lastWeek.getDate() - 7);

                    let lastMonth = new Date();
                    lastMonth.setMonth(today.getMonth() - 1);
                    let prevMonthStart = new Date();
                    prevMonthStart.setMonth(lastMonth.getMonth() - 1);

                    criminals.forEach(criminal => {
                        let addedDate = new Date(criminal.createdAt); // Ensure your API provides 'createdAt'

                        if (addedDate.toDateString() === today.toDateString()) {
                            todayCount++;
                        }
                        if (addedDate.toDateString() === yesterday.toDateString()) {
                            yesterdayCount++;
                        }
                        if (addedDate >= lastWeek) {
                            lastWeekCount++;
                        }
                        if (addedDate >= prevWeekStart && addedDate < lastWeek) {
                            prevWeekCount++;
                        }
                        if (addedDate >= lastMonth) {
                            lastMonthCount++;
                        }
                        if (addedDate >= prevMonthStart && addedDate < lastMonth) {
                            prevMonthCount++;
                        }
                    });

                    // Calculate percentage increases
                    function calculatePercentage(current, previous) {
                        if (previous === 0) return current > 0 ? 100 : 0;
                        return (((current - previous) / previous) * 100).toFixed(2);
                    }

                    let todayChange = calculatePercentage(todayCount, yesterdayCount);
                    let weekChange = calculatePercentage(lastWeekCount, prevWeekCount);
                    let monthChange = calculatePercentage(lastMonthCount, prevMonthCount);

                    // Generate cards dynamically
                    let cardsHtml = `
                        <div class="row g-6 mb-6">
                            <div class="col-sm-3 col-xl-4">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="d-flex align-items-start justify-content-between">
                                            <div class="content-left">
                                                <span class="text-heading">Criminals Today</span>
                                                <div class="d-flex align-items-center my-1">
                                                    <h4 class="mb-0 me-2">${todayCount}</h4>
                                                    <p class="mb-0 ${todayChange >= 0 ? 'text-danger' : 'text-succes'}">${todayChange}%</p>
                                                </div>
                                                <small class="mb-0">Added today</small>
                                            </div>
                                            <div class="avatar">
                                                <span class="avatar-initial rounded bg-label-primary">
                                                    <i class="ti ti-user ti-26px"></i>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-6 col-xl-4">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="d-flex align-items-start justify-content-between">
                                            <div class="content-left">
                                                <span class="text-heading">Last Week</span>
                                                <div class="d-flex align-items-center my-1">
                                                    <h4 class="mb-0 me-2">${lastWeekCount}</h4>
                                                    <p class="mb-0 ${weekChange >= 0 ? 'text-danger' : 'text-success'}">(${weekChange}%)</p>
                                                </div>
                                                <small class="mb-0">Criminals added</small>
                                            </div>
                                            <div class="avatar">
                                                <span class="avatar-initial rounded bg-label-danger">
                                                    <i class="ti ti-user-plus ti-26px"></i>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-6 col-xl-4">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="d-flex align-items-start justify-content-between">
                                            <div class="content-left">
                                                <span class="text-heading">Last Month</span>
                                                <div class="d-flex align-items-center my-1">
                                                    <h4 class="mb-0 me-2">${lastMonthCount}</h4>
                                                    <p class="mb-0 ${monthChange >= 0 ? 'text-danger' : 'text-success'}">(${monthChange}%)</p>
                                                </div>
                                                <small class="mb-0">Criminals added</small>
                                            </div>
                                            <div class="avatar">
                                                <span class="avatar-initial rounded bg-label-success">
                                                    <i class="ti ti-user-check ti-26px"></i>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;

                    statsContainer.html(cardsHtml);
                }
            },
            error: function (error) {
                console.error('Error fetching criminal statistics:', error);
            }
        });
    }

    // Call the function to populate stats on page load
    fetchCriminalStats();


    if (dt_criminal_table.length) {
        if ($.fn.dataTable.isDataTable(dt_criminal_table)) {
            dt_criminal_table.DataTable().destroy();
        }

        var dt_criminal = dt_criminal_table.DataTable({
            ajax: {
                url: '/api/CriminalAPI',
                dataSrc: 'data'
            },
            columns: [
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <button class="btn btn-sm view-details" style="background: transparent; border: none"
                            data-id="${row.id}" data-name="${row.name}"
                            data-alias="${row.alias}" data-age="${row.age}" 
                            data-gender="${row.gender}" data-description="${row.description}" 
                            data-caught="${row.caught}" 
                            data-image="/criminal/${row.imageFilename}">
                            <i class="ti ti-eye"></i>
                        </button>
                    `;
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        return `
                        <div style="display: flex; align-items: center;">
                            <img src="/criminal/${row.imageFilename}" alt="Criminal Image" width="50" height="50" style="border-radius: 50%; margin-right: 10px;">
                            ${row.name}
                        </div>
                    `;
                    }
                },
                { data: 'alias' },
                { data: 'age' },
                { data: 'gender' },
                { data: 'description' },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <a href="/Criminal/Edit/${row.id}">
                            <i class="ti ti-pencil me-1"></i> 
                        </a>
                        
                        <a href="javascript:void(0);" class="delete-criminal" data-id="${row.id}">
                            <i class="ti ti-trash me-1 text-danger"></i> 
                        </a>
                    `;
                    }
                }
            ],
            order: [[1, 'asc']],
            dom: '<"row"<"col-md-6"l><"col-md-6 d-flex justify-content-end align-items-center"fB>>t<"row"<"col-md-6"i><"col-md-6"p>>',
            language: {
                sLengthMenu: '_MENU_',
                search: '',
                searchPlaceholder: 'Search Criminal',
                paginate: {
                    next: '<i class="ti ti-chevron-right ti-sm"></i>',
                    previous: '<i class="ti ti-chevron-left ti-sm"></i>'
                }
            },
            buttons: [
                {
                    extend: 'collection',
                    className: 'btn btn-label-secondary dropdown-toggle mx-4 waves-effect waves-light',
                    text: '<i class="ti ti-upload me-2 ti-xs"></i> Export',
                    buttons: [
                        { extend: 'print', text: '<i class="ti ti-printer me-2"></i>Print', className: 'dropdown-item' },
                        { extend: 'csv', text: '<i class="ti ti-file-text me-2"></i>CSV', className: 'dropdown-item' },
                        { extend: 'excel', text: '<i class="ti ti-file-spreadsheet me-2"></i>Excel', className: 'dropdown-item' },
                        { extend: 'pdf', text: '<i class="ti ti-file-code-2 me-2"></i>PDF', className: 'dropdown-item' },
                        { extend: 'copy', text: '<i class="ti ti-copy me-2"></i>Copy', className: 'dropdown-item' }
                    ]
                }
            ]
        });
        // Handle Delete Button Click
        $(document).on('click', '.delete-criminal', function (event) {
            event.preventDefault(); // Prevent default anchor behavior

            let criminalId = $(this).data('id'); // Get Criminal ID

            Swal.fire({
                title: 'Are you sure?',
                text: "You won't be able to revert this!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'No, cancel!',
                customClass: {
                    confirmButton: 'btn btn-primary me-3 waves-effect waves-light',
                    cancelButton: 'btn btn-label-secondary waves-effect waves-light'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.value) {
                    // Perform delete operation via AJAX
                    $.ajax({
                        url: `/Criminal/Delete/${criminalId}`, // Ensure this is the correct endpoint
                        type: 'DELETE', // Use DELETE method
                        success: function (response) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Deleted!',
                                text: 'The record has been deleted.',
                                customClass: {
                                    confirmButton: 'btn btn-success waves-effect waves-light'
                                }
                            });

                            // Refresh DataTable after deletion
                            dt_criminal_table.DataTable().ajax.reload();
                        },
                        error: function (xhr, status, error) {
                            Swal.fire({
                                title: 'Error!',
                                text: 'Failed to delete the record.',
                                icon: 'error',
                                customClass: {
                                    confirmButton: 'btn btn-danger waves-effect waves-light'
                                }
                            });
                        }
                    });
                }
            });
        });

        // Open Modal on "View Details" Button Click
        $(document).on('click', '.view-details', function () {
            let name = $(this).data('name');
            let alias = $(this).data('alias');
            let age = $(this).data('age');
            let gender = $(this).data('gender');
            let description = $(this).data('description');
            let image = $(this).data('image');
            let caught = $(this).data('caught');

            // Determine badge class based on caught status
            let caughtClass = caught == 1 ? 'bg-label-success' : 'bg-label-danger';
            let caughtText = caught == 1 ? 'Caught' : 'Not Caught';

            // Set modal values
            $('#modalName').text(name);
            $('#modalAlias').text(alias);
            $('#modalAge').text(age);
            $('#modalGender').text(gender);
            $('#modalDescription').text(description);
            $('#modalImage').attr('src', image);

            // Update caught status with styling
            $('#modalCaughtStatus')
                .text(caughtText)
                .removeClass('bg-label-success bg-label-danger')
                .addClass(caughtClass);

            // Show modal
            $('#criminalModal').modal('show');
        });

     
    }
});

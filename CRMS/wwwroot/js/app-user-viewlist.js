// Global variables
let deleteModal;
let currentUserId = null;

$(document).ready(function () {
    console.log("Document ready - Initializing...");
    
    try {
        // Initialize delete modal
        const modalElement = document.getElementById('deleteUserModal');
        if (!modalElement) {
            console.error("Delete modal element not found!");
            return;
        }
        
        deleteModal = new bootstrap.Modal(modalElement);
        console.log("Delete modal initialized successfully");

        // Load initial data
        loadUsers();
        loadRoles();

        // Handle confirm delete button click
        $('#confirmDelete').on('click', function() {
            console.log("Delete button clicked, currentUserId:", currentUserId);
            if (currentUserId) {
                deleteUser(currentUserId);
                deleteModal.hide();
            } else {
                console.error("No user ID selected for deletion");
                Swal.fire({
                    title: 'Error!',
                    text: 'No user selected for deletion',
                    icon: 'error'
                });
            }
        });

    } catch (error) {
        console.error("Error during initialization:", error);
    }
});

// Function to load roles based on the selected category
function loadRoles(selectedCategory = null) {
    const roleMapping = {
        "Primary": ["Sentinel Prime", "Warden"],
        "Secondary": ["Vanguard", "Ghost"],
        "Success": ["Cipher", "Tracker"],
        "Danger": ["Cadet"],
        "Warning": ["Warden", "Tracker"],
        "Info": ["Ghost", "Cipher"],
        "Dark": ["Vanguard", "Sentinel Prime"]
    };

    var roleDropdown = $("#role");
    roleDropdown.empty();

    if (selectedCategory && roleMapping[selectedCategory]) {
        roleMapping[selectedCategory].forEach(function (role) {
            roleDropdown.append(`<option value="${role}">${role}</option>`);
        });
    } else {
        // If no category is selected, show all roles
        Object.values(roleMapping).flat().forEach(function (role) {
            roleDropdown.append(`<option value="${role}">${role}</option>`);
        });
    }
}

// Function to create a user
// Function to create a user with a spinner
function createUser() {
    var user = {
        fullName: $("#fullName").val(),
        email: $("#email").val(),
        role: $("input[name='role']:checked").val()
    };

    if (!user.role) {
        alert("Please select a role.");
        return;
    }

    // Show spinner and disable the button
    $("#loadingSpinner").show();
    $("#createUserBtn").prop("disabled", true);

    $.ajax({
        url: "/api/userapi/create",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(user),
        success: function (response) {
            alert("User created successfully. Check email for password.");
            loadUsers();
        },
        error: function (xhr) {
            alert("Error: " + (xhr.responseJSON?.message || "Something went wrong"));
        },
        complete: function () {
            // Hide spinner and enable the button after request completion
            $("#loadingSpinner").hide();
            $("#createUserBtn").prop("disabled", false);
        }
    });
}

function loadUsers() {
    console.log("Loading users...");
    $.ajax({
        url: "/api/userapi",
        type: "GET",
        success: function (data) {
            console.log("Users loaded successfully:", data);
            var rows = "";
            $.each(data, function (index, user) {
                rows += `<tr>
                    <td>${user.fullName}</td>
                    <td>${user.email}</td>
                    <td>${user.roles.join(", ")}</td>
                    <td>${new Date(user.memberSince).toLocaleDateString()}</td>
                    <td>
                       
                            <button class="btn btn-sm btn-icon btn-outline-danger" onclick="showDeleteModal('${user.id}')">
                                <i class="ti ti-trash me-1"></i>
                            </button>
                       
                    </td>
                </tr>`;
            });
            $("#userTable tbody").html(rows);
        },
        error: function (xhr, status, error) {
            console.error("Error loading users:", error);
            Swal.fire({
                title: 'Error!',
                text: 'Error fetching users: ' + error,
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    });
}

function showDeleteModal(userId) {
    console.log("Showing delete modal for user:", userId);
    try {
        currentUserId = userId;
        if (deleteModal) {
            deleteModal.show();
        } else {
            console.error("Delete modal not initialized");
            const modalElement = document.getElementById('deleteUserModal');
            deleteModal = new bootstrap.Modal(modalElement);
            deleteModal.show();
        }
    } catch (error) {
        console.error("Error showing delete modal:", error);
        Swal.fire({
            title: 'Error!',
            text: 'Error showing delete confirmation',
            icon: 'error'
        });
    }
}

function deleteUser(userId) {
    console.log("Deleting user:", userId);
    $.ajax({
        url: `/api/userapi/${userId}`,
        type: "DELETE",
        success: function (response) {
            console.log("User deleted successfully:", response);
            Swal.fire({
                title: 'Success!',
                text: 'User deleted successfully!',
                icon: 'success',
                confirmButtonText: 'OK'
            }).then((result) => {
                loadUsers();
            });
        },
        error: function (xhr, status, error) {
            console.error("Error deleting user:", error);
            Swal.fire({
                title: 'Error!',
                text: xhr.responseJSON?.message || 'Error deleting user: ' + error,
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    });
}

$(function () {
    var dt_user_table = $('.datatables-users');

    if (dt_user_table.length) {
        if ($.fn.dataTable.isDataTable(dt_user_table)) {
            dt_user_table.DataTable().destroy();
        }

        var dt_user = dt_user_table.DataTable({
            ajax: {
                url: '/api/userapi',
                dataSrc: ''
            },
            columns: [
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <button class="btn btn-sm view-details" style="background: transparent; border: none"
                            data-id="${row.id}" data-fullname="${row.fullName}"
                            data-email="${row.email}" data-roles="${row.roles.join(', ')}" 
                            data-membersince="${row.memberSince}">
                            <i class="ti ti-eye"></i>
                        </button>
                    `;
                    }
                },
                { data: 'fullName' },
                { data: 'email' },
                { 
                    data: 'roles',
                    render: function(data) {
                        return data.join(', ');
                    }
                },
                { 
                    data: 'memberSince',
                    render: function(data) {
                        return new Date(data).toLocaleDateString();
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <a href="javascript:void(0);" class="delete-user" data-id="${row.id}">
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
                searchPlaceholder: 'Search User',
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

        // Handle View Details Button Click
        $(document).on('click', '.view-details', function () {
            const userId = $(this).data('id');
            const fullName = $(this).data('fullname');
            const email = $(this).data('email');
            const roles = $(this).data('roles');
            const memberSince = new Date($(this).data('membersince')).toLocaleDateString();

            $('#modalFullName').text(fullName);
            $('#modalEmail').text(email);
            $('#modalRoles').text(roles);
            $('#modalMemberSince').text(memberSince);

            const userModal = new bootstrap.Modal(document.getElementById('userModal'));
            userModal.show();
        });

        // Handle Delete Button Click
        $(document).on('click', '.delete-user', function (event) {
            event.preventDefault();
            const userId = $(this).data('id');

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
                    $.ajax({
                        url: `/api/userapi/${userId}`,
                        type: 'DELETE',
                        success: function () {
                            Swal.fire({
                                title: 'Deleted!',
                                text: 'User has been deleted.',
                                icon: 'success',
                                customClass: {
                                    confirmButton: 'btn btn-success waves-effect waves-light'
                                }
                            }).then(() => {
                                dt_user.ajax.reload();
                            });
                        },
                        error: function () {
                            Swal.fire({
                                title: 'Error!',
                                text: 'Error deleting user.',
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
    }
});

$(document).ready(function () {
    loadUsers();
    loadRoles(); // Load default roles initially

    // Listen for changes in radio button selection
    
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

// Function to load users
function loadUsers() {
    $.ajax({
        url: "/api/userapi",
        type: "GET",
        success: function (data) {
            var rows = "";
            $.each(data, function (index, user) {
                rows += `<tr>
                    <td>${user.fullName}</td>
                    <td>${user.email}</td>
                    <td>${user.roles.join(", ")}</td>
                    <td>${new Date(user.memberSince).toLocaleDateString()}</td>
                    <td>
                        <button onclick="deleteUser('${user.id}')">Delete</button>
                    </td>
                </tr>`;
            });
            $("#userTable tbody").html(rows);
        },
        error: function () {
            alert("Error fetching users");
        }
    });
}

// Function to delete a user
function deleteUser(userId) {
    if (!confirm("Are you sure you want to delete this user?")) return;

    $.ajax({
        url: `/api/userapi/${userId}`,
        type: "DELETE",
        success: function () {
            alert("User deleted successfully!");
            loadUsers();
        },
        error: function () {
            alert("Error deleting user");
        }
    });
}

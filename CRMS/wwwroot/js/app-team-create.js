    $(document).ready(function () {
        loadUsers();

    $("#searchUsers").on("input", function () {
        filterUsers($(this).val());
        });

    function loadUsers() {
        $.ajax({
            url: "/api/TeamApi/users",
            type: "GET",
            success: function (data) {
                window.users = data; // Store users globally for filtering
                displayUsers(data);
            },
            error: function (xhr) {
                console.error("Error loading users:", xhr.responseText);
                alert("Error loading users: " + xhr.status + " " + xhr.statusText);
            }
        });
        }

    function displayUsers(users) {
            var userList = $("#userList");
    userList.empty();
            users.forEach(user => {
        userList.append(`
                    <div class="user-item">
                        <input type="checkbox" class="user-checkbox" data-id="${user.id}" data-name="${user.fullName}">
                        <label>${user.fullName}</label>
                    </div>
                `);
            });

    // Add click event to checkboxes
    $(".user-checkbox").on("change", function () {
                var userId = $(this).data("id");
    var userName = $(this).data("name");
    if ($(this).is(":checked")) {
        addSelectedMember(userId, userName);
                } else {
        removeSelectedMember(userId);
                }
            });
        }

    function filterUsers(query) {
            var filteredUsers = window.users.filter(user => user.fullName.toLowerCase().includes(query.toLowerCase()));
    displayUsers(filteredUsers);
        }

    function addSelectedMember(id, name) {
        $("#selectedMembers").append(`
                <div class="selected-member" data-id="${id}">
                    <span>${name}</span>
                    <button class="remove-member" data-id="${id}">Remove</button>
                </div>
            `);

    // Add click event to remove button
    $(".remove-member").on("click", function () {
                var memberId = $(this).data("id");
    removeSelectedMember(memberId);
            });
        }

    function removeSelectedMember(id) {
        $(`#selectedMembers .selected-member[data-id="${id}"]`).remove();
    $(`.user-checkbox[data-id="${id}"]`).prop("checked", false); // Uncheck the checkbox
        }

    $("#createTeamForm").submit(function (event) {
        event.preventDefault();
    createTeam();
        });

    function createTeam() {
            var teamData = {
        name: $("#teamName").val(),
    teamMembers: $("#selectedMembers .selected-member").map(function () {
                    return $(this).data("id");
                }).get() // Get selected member IDs
            };

    console.log("Sending data:", teamData);

    $.ajax({
        url: "/api/TeamApi/create",
    type: "POST",
    contentType: "application/json",
    headers: {
        "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
    data: JSON.stringify(teamData),
    success: function () {
        alert("Team created successfully!");
    window.location.href = "/Team/Index";
                },
    error: function (xhr) {
        console.error("Error creating team:", xhr.responseText);
    alert("Error creating team: " + xhr.status + " " + xhr.statusText);
                }
            });
        }
    });

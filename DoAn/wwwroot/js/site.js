const toggle =
    document.getElementById("theme-toggle");

const icon =
    document.getElementById("theme-icon");

if (localStorage.getItem("theme") === "light") {
    document.body.classList.add("light");

    icon.className =
        "bi bi-moon-fill";
}

toggle?.addEventListener("click", () => {

    document.body.classList.toggle("light");

    if (document.body.classList.contains("light")) {
        localStorage.setItem(
            "theme",
            "light"
        );

        icon.className =
            "bi bi-moon-fill";
    }
    else {
        localStorage.setItem(
            "theme",
            "dark"
        );

        icon.className =
            "bi bi-sun-fill";
    }
});
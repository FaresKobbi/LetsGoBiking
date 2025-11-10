const burgerMenu = document.getElementById('divParent');
const asideMenu = document.getElementById('sideMenu');

let isMenuOpen = false;

burgerMenu.addEventListener('click', function() {
  if (isMenuOpen) {
    asideMenu.classList.remove('popOutAnim');
    asideMenu.classList.add('popInAnim');
    burgerMenu.classList.remove('X');
    burgerMenu.classList.add('normal');
  } else {
    asideMenu.classList.remove('popInAnim');
    asideMenu.classList.add('popOutAnim');
    burgerMenu.classList.remove('normal');
    burgerMenu.classList.add('X');
  }

  isMenuOpen = !isMenuOpen;
});

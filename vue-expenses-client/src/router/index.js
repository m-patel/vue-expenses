import Vue from 'vue'
import Router from 'vue-router'
import Login from '@/views/Login.vue'
import HomePage from '@/views/HomePage.vue'
import Dashboard from '@/views/Dashboard.vue'
import Expenses from '@/views/Expenses.vue'
import Stats from '@/views/Stats.vue'
import Settings from '@/views/Settings.vue'
import Profile from '@/views/Profile.vue'

Vue.use(Router)

const router = new Router({
  mode: 'history',
  routes: [
    {
      path: '/', component: HomePage,
      children: [
        //HomePage's <router-view>
        { path: '/dashboard', component: Dashboard },
        { path: '/expenses', component: Expenses },     
        { path: '/stats', component: Stats },
        { path: '/settings', component: Settings },
        { path: '/profile', component: Profile },
        { path: '/', component: Dashboard },
      ]
    },
    { path: '/login', component: Login },
    // otherwise redirect to home
    { path: '*', redirect: '/' }
  ]
});

router.beforeEach((to, from, next) => {
  // redirect to login page if not logged in and trying to access a restricted page
  const publicPages = ['/login'];
  const authRequired = !publicPages.includes(to.path);
  const loggedIn = localStorage.getItem('user');

  if (authRequired && !loggedIn) {
    //return next('/login');
  }

  next();
})

export default router;
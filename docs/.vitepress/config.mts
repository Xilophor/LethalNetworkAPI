import {defineConfig} from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
    base: "/LethalNetworkAPI/",
    lang: 'en-US',
    title: "LethalNetworkAPI Wiki",
    description: "How to use the LethalNetworkingAPI.",
    cleanUrls: true,
    themeConfig: {
        // https://vitepress.dev/reference/default-theme-config
        nav: [
            { text: 'Home', link: '/' },
            { text: 'API Docs', link: '/overview' }
        ],

        sidebar: [
            { text: 'Referencing the API', link: '/overview' },
            {
                text: 'Network Messages',
                items: [
                    { text: 'Server Message', link: '/messages/server' },
                    { text: 'Client Message', link: '/messages/client' }
                ]
            },
            {
                text: 'Network Events',
                items: [
                    { text: 'Server Event', link: '/events/server' },
                    { text: 'Client Event', link: '/events/client' }
                ]
            },
            { text: 'Network Variables', items: [
                    { text: 'Usage', link: '/variables/usage' },
                    { text: 'Ownership', link: '/variables/ownership' },
                ]
            },
            { text: 'Extensions', link: '/extensions' },
        ],

        socialLinks: [
            { icon: 'github', link: 'https://github.com/Xilophor/LethalNetworkAPI' },
            {
                icon:
                {
                    svg:
                        "<svg role=\"img\" viewBox=\"0 0 24 24\" xmlns=\"http://www.w3.org/2000/svg\">" +
                        "<title>NuGet</title>" +
                        "<path d=\"M17.67 21.633a3.995 3.995 0 1 1 0-7.99 3.995 3.995 0 0 1 0 7.99m-7.969-9.157a2.497 2.497 0 1 1 0-4.994 2.497 2.497 0 0 1 0 4.994m8.145-7.795h-6.667a6.156 6.156 0 0 0-6.154 6.155v6.667a6.154 6.154 0 0 0 6.154 6.154h6.667A6.154 6.154 0 0 0 24 17.503v-6.667a6.155 6.155 0 0 0-6.154-6.155M3.995 2.339a1.998 1.998 0 1 1-3.996 0 1.998 1.998 0 0 1 3.996 0\"/>" +
                        "</svg>"
                },
                link: 'https://www.nuget.org/',
                ariaLabel: 'nuget'
            },
            {
                icon:
                    {
                        svg:
                            "<svg role=\"img\" viewBox=\"0 0 1000 896\" xmlns=\"http://www.w3.org/2000/svg\">\n" +
                            "<title>Thunderstore</title>" +
                            "<path d=\"M13.4223 496.845L209.485 838.17L300 650.202L200.99 477.966C189.992 458.897 189.992 436.945 200.99 417.779L324.555 202.755C335.561 183.611 354.447 172.666 376.421 172.675H442.857L314.286 462.366H473.143L257.143 881.384L690.941 361.014H557.588L648.593 172.675H808.03H900.762L1000 2.28882e-05H715.868H526.836H298.96C263.138 0.0084323 232.393 17.8324 214.461 48.9346L13.4223 398.9C-4.46781 430.078 -4.48036 465.827 13.4223 496.845ZM313.959 895.833H701.066C736.813 895.833 767.63 878.005 785.612 846.819L986.655 496.836C1004.44 465.827 1004.44 430.078 986.655 398.892L906.26 258.947H707.808L799.079 417.779C809.985 436.961 809.984 458.91 799.049 477.974L675.531 693.049C664.454 712.222 645.555 723.15 623.555 723.15H533.795L471.429 722.446L313.959 895.833Z\"/>\n" +
                            "</svg>"
                    },
                link: 'https://thunderstore.io/c/lethal-company/p/xilophor/LethalNetworkAPI',
                ariaLabel: 'thunderstore'
            },
        ]
    }
})

// Next.js API route support: https://nextjs.org/docs/api-routes/introduction
import type { NextApiRequest, NextApiResponse } from 'next';
import httpProxy, { ProxyResCallback } from 'http-proxy';
import Cookies from 'cookies';

type Data = {
  message: string;
};

const proxy = httpProxy.createProxyServer();
export const config = {
  api: {
    bodyParser: false,
  },
};
export default function handler(req: NextApiRequest, res: NextApiResponse<Data>) {
  if (req.method !== 'POST') {
    return res.status(404).json({ message: 'Method not supported' });
  }

  return new Promise((resolve) => {
    const handlerProxyRes: ProxyResCallback = (proxyRes, req, res) => {
      let body = '';
      proxyRes.on('data', function (chunk) {
        body += chunk;
      });
      proxyRes.on('end', function () {
        try {
          const { token, expr } = JSON.parse(body);

          // convert token to cookies
          var cookies = new Cookies(req, res, { secure: true });
          cookies.set('access_token', token, {
            httpOnly: true,
            sameSite: 'lax',
            expires: new Date(expr),
          });

          (res as NextApiResponse).status(200).json({ message: 'Login Success' });
        } catch (error) {
          (res as NextApiResponse).status(500).json({ message: 'Login Fail' });
          console.error(error);
        }
        resolve(true);
      });
    };
    proxy.once('proxyRes', handlerProxyRes);
    proxy.web(req, res, {
      target: process.env.API_URL,
      changeOrigin: true,
      selfHandleResponse: true,
    });
  });
  //   res.status(200).json({ name: "John Doe" });
}
